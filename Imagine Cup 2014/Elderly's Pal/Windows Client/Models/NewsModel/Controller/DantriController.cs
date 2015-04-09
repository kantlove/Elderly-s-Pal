using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Net;

namespace NiceDreamers.Windows.Models
{
    public class DantriController
    {
        public static async Task DoSearch(NewsItemList plist, string keyword)
        {
            try
            {
                plist.Clear();
                string url = "http://search.dantri.com.vn/SearchResult.aspx?s=" + keyword + "&PageIndex=1";
                await LoadItemsFromPage(plist, url, "Dantri", null);
                if (plist.Count == 0)
                    MessageBox.Show("Sorry, no articles found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("System got an error at DoSearch function with message:\n" + ex.Message);
                return;
            }
        }
        public static void ptichDate(NewsItem item)
        {
            item.DatePublished = HtmlDownloader.removeHtml(item.DatePublished);
            string thuu = item.DatePublished.Replace(", ", ",");
            thuu = thuu.Replace(" - ", "-");
            string[] thu = thuu.Split(',');
            if (thu.Length > 1)
            {
                string date = thu[1];
                string[] time = date.Split('-');
                if (time.Length == 2)
                {
                    time[0] = HtmlDownloader.removeHtml(time[0]);
                    time[1] = HtmlDownloader.removeHtml(time[1]);
                    string[] day = time[0].Split('/');
                    if (day.Length == 3)
                    {
                        if (day[1].Length == 1) day[1] = "0" + day[1];
                        if (day[0].Length == 1) day[0] = "0" + day[1];
                        time[0] = day[2] + day[1] + day[0];
                    }
                    item.DateStandard = time[0] + time[1];
                }
            }
        }
        //hàm lấy content của một article
        public static async Task LoadContentFrom(NewsItem item, string url)
        {      
            if (!url.Contains("http://"))
                url = "http://dantri.com.vn" + url;
            string html = await HtmlDownloader.loadFromUrl(url);
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(html);
            HtmlNode nodeContent = page.DocumentNode.SelectSingleNode("//div[@class='fon34 mt3 mr2 fon43']");
            HtmlNode nodeTag = page.DocumentNode.SelectSingleNode("//div[@class='news-tag']");
            if (nodeTag != null)
            {
                var allNodeInTag = nodeTag.SelectNodes(nodeTag.XPath + "//a[@href]");
                if (allNodeInTag != null)
                {
                    foreach (HtmlNode node in allNodeInTag)
                    {
                        ItemTag tag = new ItemTag()
                        {
                            Title = HtmlDownloader.removeHtml(node.InnerText),
                            Link = node.Attributes["href"].Value
                        };
                        item.addToTagList(tag);
                        tag = null;
                    }
                }
            }

            
            //disable all link
            var allNodeInContent = nodeContent.SelectNodes(nodeContent.XPath + "//*[@href]");
            if (allNodeInContent != null)
            {
                foreach (HtmlNode node in allNodeInContent)
                {
                    node.SetAttributeValue("href", ""); //remove reference link
                }
            }
            int positionToDel = nodeContent.InnerHtml.IndexOf("<div class=\"news-tag\">");
            if (positionToDel>0)
                nodeContent.InnerHtml = nodeContent.InnerHtml.Substring(0,positionToDel);
            HtmlNode nodeTime = page.DocumentNode.SelectSingleNode("//span[@class='fr fon7 mr2']");
            item.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
            ptichDate(item);
            item.Content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>";
            item.Content += "<p>" + item.DatePublished + "</p>";
            item.Content += "<p><b>Tóm tắt nội dung:</b> <i>" + item.ShortContent + "</i></p>";
            item.Content += nodeContent.InnerHtml.Replace(nodeTag.OuterHtml, "");            
            item.Content = WebUtility.HtmlEncode(item.Content);
           
        }
        //hàm tạo 1 list category
        public static void initCategory(ObservableCollection<Category> catList)
        {
            catList.Add(new Category() { Title = "Sự kiện", Link = "http://dantri.com.vn/su-kien.htm" });
            catList.Add(new Category() { Title = "Giáo dục", Link = "http://dantri.com.vn/giao-duc-khuyen-hoc.htm" });
            catList.Add(new Category() { Title = "Thế giới", Link = "http://dantri.com.vn/the-gioi.htm" });
            catList.Add(new Category() { Title = "Kinh doanh", Link = "http://dantri.com.vn/kinh-doanh.htm" });
            catList.Add(new Category() { Title = "Công nghệ số", Link = "http://dantri.com.vn/suc-manh-so.htm" });
            catList.Add(new Category() { Title = "Bạn đọc viết", Link = "http://dantri.com.vn/ban-doc.htm" });
            catList.Add(new Category() { Title = "Giải trí", Link = "http://dantri.com.vn/giai-tri.htm" });
            catList.Add(new Category() { Title = "Pháp luật", Link = "http://dantri.com.vn/phap-luat.htm" });
            catList.Add(new Category() { Title = "Ô tô - xe máy", Link = "http://dantri.com.vn/o-to-xe-may.htm" });
            catList.Add(new Category() { Title = "Thể thao", Link = "http://dantri.com.vn/the-thao.htm" });
        }
        public static bool getHottestNew(HtmlDocument page, NewsItem hottest)
        {
            if (hottest == null)
                return false;
            HtmlNode itemNode = page.DocumentNode.SelectSingleNode("//div[@class='nbmda mt3 clearfix']");
            if (itemNode == null)
            {
                return false;
            }
            NewsItem article = hottest;
            HtmlNode nodeTitle = itemNode.SelectSingleNode(itemNode.XPath + "//a[@href and @title]");
            HtmlNode nodeImage = itemNode.SelectSingleNode(itemNode.XPath + "//img[@src]");
            if (nodeImage != null)
            {
                article.ImageLink = nodeImage.Attributes["src"].Value;
            }
            
            article.LinkUrl = nodeTitle.Attributes["href"].Value;
            article.Name = HtmlDownloader.removeHtml(nodeTitle.Attributes["title"].Value);
            HtmlNode nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//div[@class='fon5 wid255 fl']");
            //cut the related link out
            int cutIndex = nodeShortContent.InnerHtml.IndexOf("<br");

            string shortContent = nodeShortContent.InnerHtml;
            //if find the trash
            if (cutIndex > 0)
                shortContent = nodeShortContent.InnerHtml.Substring(0, cutIndex);
            //modify the trash in the content
            shortContent = shortContent.Replace("&gt;", "");
            article.ShortContent = HtmlDownloader.removeHtml(shortContent);
            return true;
        }
        public static async Task LoadItemsFromTag(NewsItemList plist, ItemTag tag)
        {
            await LoadItemsFromPage(plist, tag.Link, "Dantri", null);
        }
        //hàm lấy một list các article từ 1 url của category
        public static async Task LoadItemsFromPage(NewsItemList plist, string url, string source, NewsItem hottest = null)
        {
            try
            {
                if (!url.Contains("http://"))
                    url = "http://dantri.com.vn" + url;
                plist.Clear();
                if (hottest != null)
                {
                    hottest.Source = source;
                }
                string html = await HtmlDownloader.loadFromUrl(url);
                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(html);
                var allItemNodes = page.DocumentNode.SelectNodes("//div[@class='mt3 clearfix']");
                
                if (getHottestNew(page, hottest))
                {
                    if (hottest != null)
                        plist.Add(hottest);
                }
                if (allItemNodes == null)
                    return;
                foreach (HtmlNode itemNode in allItemNodes)
                {
                    NewsItem article = new NewsItem();
                    article.Source = source;
                    HtmlNode nodeImage = itemNode.SelectSingleNode(itemNode.XPath + "//img[@src]");
                    if (nodeImage != null)
                    {
                        article.ImageLink = nodeImage.Attributes["src"].Value;
                    }
                    var nodeTitle = itemNode.SelectNodes(itemNode.XPath + "//a");
                    foreach (HtmlNode node in nodeTitle)
                    {
                        if (node.Attributes.Contains("href"))
                            if (article.LinkUrl==null||article.LinkUrl=="")
                                article.LinkUrl = node.Attributes["href"].Value;
                        if (node.Attributes.Contains("title"))
                            if (article.Name==null||article.Name=="")
                                article.Name = HtmlDownloader.removeHtml(node.Attributes["title"].Value);
                        if (!node.InnerHtml.Contains("<img"))
                            if (node.InnerText.Length>10)
                                if (article.Name == null || article.Name == "")
                                    article.Name = HtmlDownloader.removeHtml(node.InnerText);
                    }                    
                    if (article.LinkUrl.Contains("tuyensinh.dantri"))
                    {
                        continue;
                    }                                        
                    
                    HtmlNode nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//div[@class='fon5 wid324 fl']");
                    if (nodeShortContent == null)
                        nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//div[@class='fon5 fl']");
                    if (nodeShortContent != null)
                    {
                        //cut the related link out
                        int cutIndex = nodeShortContent.InnerHtml.IndexOf("<br");
                        string shortContent = nodeShortContent.InnerHtml;
                        //if find the trash
                        if (cutIndex > 0)
                            shortContent = nodeShortContent.InnerHtml.Substring(0, cutIndex);
                        //modify the trash in the content
                        shortContent = shortContent.Replace("&gt;", "");
                        article.ShortContent = HtmlDownloader.removeHtml(shortContent);
                    }
                    plist.Add(article);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("System got an error at LoadItemsFromPage function with message:\n" + ex.Message);
                return;
            }            
        }

    }
}
