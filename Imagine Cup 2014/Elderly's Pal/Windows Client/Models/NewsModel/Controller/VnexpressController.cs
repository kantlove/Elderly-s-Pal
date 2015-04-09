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
    public class VnexpressController
    {
        public static async Task DoSearch(NewsItemList plist, string keyword)
        {
            try
            {
                plist.Clear();
                string url = "http://timkiem.vnexpress.net/?q=" + keyword;
                string html = await HtmlDownloader.loadFromUrl(url);
                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(html);
                var allResultNodes = page.DocumentNode.SelectNodes("//li[@class='block_search_result_text']");
                if (allResultNodes == null)
                {
                    MessageBox.Show("Sorry, no articles found!");
                    return;
                }
                foreach (HtmlNode itemNode in allResultNodes)
                {
                    NewsItem article = new NewsItem();
                    article.Source = "Vnexpress";
                    HtmlNode nodeLink = itemNode.SelectSingleNode(itemNode.XPath + "//a[@href]");
                    HtmlNode nodeImage = itemNode.SelectSingleNode(itemNode.XPath + "//img[@src]");
                    if (nodeImage != null)
                        article.ImageLink = nodeImage.Attributes["src"].Value;
                    article.Name = HtmlDownloader.removeHtml(nodeLink.Attributes["alt"].Value);
                    article.LinkUrl = nodeLink.Attributes["href"].Value;
                    HtmlNode nodeTime = itemNode.SelectSingleNode(itemNode.XPath + "//p[@class='txt_gray txt_11 ex_hi']");
                    article.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerHtml);
                    HtmlNode nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//span[@class='hightlight']");
                    article.ShortContent = HtmlDownloader.removeHtml(nodeShortContent.InnerText);
                    plist.Add(article);
                }
                if (plist.Count == 0)
                    MessageBox.Show("Sorry, no articles found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("System got an error at DoSearch function with message:\n" + ex.Message);
                return;
            }
        }
        //load content from ngoisao.net
        public async static Task LoadFromNgoiSao(HtmlDocument page, NewsItem item)
        {
            //load time
            HtmlNode nodeTime = page.DocumentNode.SelectSingleNode("//span[@class='spanDateTime fl']");
            if (nodeTime != null)
            {
                item.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
                ptichDate(item);
            }
            //load item tag
            HtmlNode nodeTag = page.DocumentNode.SelectSingleNode("//div[@class='wordTag']");
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
            //load item succient content
            HtmlNode nodeDescription = page.DocumentNode.SelectSingleNode("//h2[@class='lead']");
            if (nodeDescription!=null)
                item.ShortContent = HtmlDownloader.removeHtml(nodeDescription.InnerText);
            HtmlNode nodeId = page.DocumentNode.SelectSingleNode("//meta[@name='tt_article_id']");
            string id = nodeId.Attributes["content"].Value;
            string html = await HtmlDownloader.loadFromUrl("http://ngoisao.net/detail/print?id=" + id);
            HtmlDocument printPage = new HtmlDocument();
            printPage.LoadHtml(html);
            //load page content
            HtmlNode nodeContent = printPage.DocumentNode.SelectSingleNode("//div[@class='fck_detail']");
            var allNodeInContent = nodeContent.SelectNodes(nodeContent.XPath + "//*[@href]");
            if (allNodeInContent != null)
            {
                foreach (HtmlNode node in allNodeInContent)
                {
                    node.SetAttributeValue("href", ""); //remove reference link                    
                }
            }
            item.Content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>";
            item.Content += "<p>" + item.DatePublished + "</p>";
            item.Content += "<p><b>Tóm tắt nội dung:</b> <i>" + item.ShortContent + "</i></p>";
            item.Content += nodeContent.InnerHtml;
            item.Content = WebUtility.HtmlEncode(item.Content);                                    
        }
        //load content from ione.vnexpress.net
        private static void LoadFromIOne(HtmlDocument page, NewsItem item)
        {
            //load time
            HtmlNode nodeTime = page.DocumentNode.SelectSingleNode("//div[@class='time left']");
            if (nodeTime != null)
            {
                string date = HtmlDownloader.removeHtml(nodeTime.InnerText);
                item.DatePublished = date;
                date = date.Replace("AM", "");
                date = date.Replace("PM", "");
                string[] time = date.Split('|');
                if (time.Length == 2)
                {
                    time[0] = HtmlDownloader.removeHtml(time[0]);
                    time[1] = HtmlDownloader.removeHtml(time[1]);
                    string[] day = time[1].Split('/');
                    if (day.Length == 3)
                    {
                        if (day[1].Length == 1) day[1] = "0" + day[1];
                        if (day[0].Length == 1) day[0] = "0" + day[1];
                        time[1] = day[2] + day[1] + day[0];
                    }
                    item.DateStandard = time[1] + time[0];
                }                
            }
            //load item tag
            HtmlNode nodeTag = page.DocumentNode.SelectSingleNode("//div[@class='left w600 list_tags']");
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

            //load item succient content
            HtmlNode nodeDescription = page.DocumentNode.SelectSingleNode("//div[@class='lead']");
            item.ShortContent = HtmlDownloader.removeHtml(nodeDescription.InnerText);
            //load item content
            HtmlNode nodeContent = page.DocumentNode.SelectSingleNode("//div[@class='content']");
            var allNodeInContent = nodeContent.SelectNodes(nodeContent.XPath + "//*[@href]");
            if (allNodeInContent != null)
            {
                foreach (HtmlNode node in allNodeInContent)
                {
                    node.SetAttributeValue("href", ""); //remove reference link                    
                }
            }
            item.Content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>";
            item.Content += "<p>" + item.DatePublished + "</p>";
            item.Content += "<p><b>Tóm tắt nội dung:</b> <i>" + item.ShortContent + "</i></p>";
            item.Content += nodeContent.InnerHtml;
            item.Content = WebUtility.HtmlEncode(item.Content);                                                
        }
        public static async Task LoadItemsFromTag(NewsItemList plist, ItemTag tag)
        {
            if (tag == null)
                return;
            await DoSearch(plist, tag.Title);            
        }
        private static void ptichDate(NewsItem item)
        {
            item.DatePublished = HtmlDownloader.removeHtml(item.DatePublished);
            string thuu = item.DatePublished.Replace(", ", ",");
            thuu = thuu.Replace(" GMT+7","");
            string[] thu = thuu.Split(',');
            if (thu.Length > 1)
            {
                string date = thu[1];
                string[] time = date.Split(' ');
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
        public async static Task LoadContentFrom(NewsItem item, string url)
        {            
            if (!url.Contains("http://"))
                url = "http://vnexpress.net" + url;
            string html = await HtmlDownloader.loadFromUrl(url);
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(html);
            if (url.Contains("ione.vnexpress"))
                LoadFromIOne(page, item);
            else if (url.Contains("ngoisao.net"))
                await LoadFromNgoiSao(page, item);
            else
            {

                HtmlNode nodeTag;
                if (url.Contains("http://vnexpress.net"))
                {
                    nodeTag = page.DocumentNode.SelectSingleNode("//div[@class='tag-pos']");
                }
                else
                {
                    nodeTag = page.DocumentNode.SelectSingleNode("//div[@class='content_tagbar']");
                }
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
                if (url.Contains("http://vnexpress.net"))
                {
                    HtmlNode nodeDescription = page.DocumentNode.SelectSingleNode("//h2[@class='Lead']");
                    if (nodeDescription != null)
                    {
                        item.ShortContent = HtmlDownloader.removeHtml(nodeDescription.InnerText);
                    }
                    HtmlNode nodeTime = page.DocumentNode.SelectSingleNode("//span[@class='spanTime']");
                    if (nodeTime != null)
                    {
                        item.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
                        ptichDate(item);
                    }
                }
                else
                {                    
                    HtmlNode nodeDescription = page.DocumentNode.SelectSingleNode("//div[@class='short_intro']");
                    item.ShortContent = HtmlDownloader.removeHtml(nodeDescription.InnerText);
                    HtmlNode nodeTime = page.DocumentNode.SelectSingleNode("//div[@class='time txt_666 left txt_11']");
                    if (nodeTime != null)
                    {
                        item.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
                        ptichDate(item);
                    }
                }
                HtmlNode nodeContent = page.DocumentNode.SelectSingleNode("//div[@class='fck_detail']");
                //disable all link
                if (nodeContent != null)
                {
                    var allNodeInContent = nodeContent.SelectNodes(nodeContent.XPath + "//*[@href]");
                    if (allNodeInContent != null)
                    {
                        foreach (HtmlNode node in allNodeInContent)
                        {
                            node.SetAttributeValue("href", ""); //remove reference link
                        }
                    }
                    item.Content = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>";
                    item.Content += "<p>" + item.DatePublished + "</p>";
                    item.Content += "<p><b>Tóm tắt nội dung:</b> <i>" + item.ShortContent + "</i></p>";
                    item.Content += nodeContent.InnerHtml;
                    item.Content = WebUtility.HtmlEncode(item.Content);
                }
            }
        }
        public static void initCategory(ObservableCollection<Category> catList)
        {
            catList.Add(new Category() { Title = "Chính trị xã hội", Link = "http://vnexpress.net/tin-tuc/xa-hoi" });
            catList.Add(new Category() { Title = "Giáo dục", Link = "http://vnexpress.net/tin-tuc/xa-hoi/giao-duc" });            
            catList.Add(new Category() { Title = "Thế giới", Link = "http://vnexpress.net/tin-tuc/the-gioi" });            
            catList.Add(new Category() { Title = "Bạn đọc viết", Link = "http://vnexpress.net/tin-tuc/ban-doc-viet" });            
            catList.Add(new Category() { Title = "Pháp luật", Link = "http://vnexpress.net/tin-tuc/phap-luat" });
            catList.Add(new Category() { Title = "Ô tô - xe máy", Link = "http://vnexpress.net/tin-tuc/oto-xe-may" });
            catList.Add(new Category() { Title = "Khoa học", Link = "http://vnexpress.net/tin-tuc/khoa-hoc" });
        }
        public static void getHottestNew(HtmlDocument page, NewsItem hottest)
        {
            if (hottest == null) 
                return;
            HtmlNode itemNode = page.DocumentNode.SelectSingleNode("//div[@class='folder-top']");
            NewsItem article = hottest;
            HtmlNode nodeLink = itemNode.SelectSingleNode(itemNode.XPath + "//a[@href]");
            HtmlNode nodeImage = nodeLink.SelectSingleNode(itemNode.XPath + "//img[@src and @class='img-topsubject']");
            if (nodeImage != null)
            {
                article.ImageLink = nodeImage.Attributes["src"].Value;
            }            
            article.LinkUrl = nodeLink.Attributes["href"].Value;
            HtmlNode nodeTitle = itemNode.SelectSingleNode(itemNode.XPath + "//h1[@class='titleHN']");
            article.Name = HtmlDownloader.removeHtml(nodeTitle.InnerText);
            HtmlNode nodeTime = itemNode.SelectSingleNode(itemNode.XPath + "//span[@class='timeListHome']");
            article.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
            HtmlNode nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//h2[@class='h2leadHN']");
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
        
        //load articles from source, category, url
        public static async Task LoadItemsFromPage(NewsItemList plist, string url, string source, NewsItem hottest = null)
        {
            try
            {
                if (!url.Contains("vnexpress.net"))
                    url = "http://vnexpress.net" + url;
                plist.Clear();
                if (hottest != null)
                {
                    hottest.Source = source;
                }
                string html = await HtmlDownloader.loadFromUrl(url);
                HtmlDocument page = new HtmlDocument();
                page.LoadHtml(html);
                var allItemNodes = page.DocumentNode.SelectNodes("//div[@class='folder-news']");
                getHottestNew(page, hottest);
                if (hottest!=null)
                    plist.Add(hottest);
                foreach (HtmlNode itemNode in allItemNodes)
                {
                    NewsItem article = new NewsItem();
                    article.Source = source;                    
                    HtmlNode nodeImage = itemNode.SelectSingleNode(itemNode.XPath + "//img[@src]");
                    if (nodeImage != null)
                    {
                        article.ImageLink = nodeImage.Attributes["src"].Value;
                    }                        
                    HtmlNode nodeTitle = itemNode.SelectSingleNode(itemNode.XPath + "//a[@class='link-title14' and @href]");
                    article.Name = HtmlDownloader.removeHtml(nodeTitle.InnerText);
                    article.LinkUrl = nodeTitle.Attributes["href"].Value;
                    HtmlNode nodeTime = itemNode.SelectSingleNode(itemNode.XPath + "//span[@class='timeListHome']");
                    if (nodeTime != null)
                    {
                        article.DatePublished = HtmlDownloader.removeHtml(nodeTime.InnerText);
                    }
                    HtmlNode nodeShortContent = itemNode.SelectSingleNode(itemNode.XPath + "//h3[@class='h3Lead']");
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
