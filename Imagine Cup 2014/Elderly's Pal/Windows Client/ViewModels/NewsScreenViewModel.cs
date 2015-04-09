using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using Microsoft.Kinect.Toolkit.Controls;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.NewsScreen)]
    public class NewsScreenViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public static string AnalyzeArticle(string s)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(s);
            string sourceDefault =
                "<html><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" /><title>title_goes_here</title></head><body>content_title_here <font size='6'>content_goes_here</font></body></html>";
            var nodeTitle = doc.DocumentNode.SelectSingleNode("//h1[@class='mainTitle']");
            if (nodeTitle != null)
            {
                sourceDefault = sourceDefault.Replace("title_goes_here", nodeTitle.InnerText);
                nodeTitle = nodeTitle.ParentNode;
                sourceDefault = sourceDefault.Replace("content_title_here", nodeTitle.OuterHtml);
            }
            var nodeContent = doc.DocumentNode.SelectSingleNode("//div[@class='article-content article-content03']");
            if (nodeContent != null)
            {
                var allImgNodes = nodeContent.SelectNodes(nodeContent.XPath + "//img[@src]");
                if (allImgNodes!=null && allImgNodes.Any())
                foreach (var node in allImgNodes)
                {
                    string link = node.Attributes["src"].Value;
                    if (!link.Contains("http://"))
                    {
                        node.SetAttributeValue("src", "http://www.thanhnien.com.vn" + link);
                    }
                }
                var allTagNodes =
                    nodeContent.SelectNodes(nodeContent.XPath +
                                            "//a[@style='color:#d00 !important;font-weight:bold;text-decoration:none']");
                if (allTagNodes!=null && allTagNodes.Any())
                foreach (var node in allTagNodes)
                {
                    node.Remove();
                }
                sourceDefault = sourceDefault.Replace("content_goes_here", nodeContent.OuterHtml);
            }
            return sourceDefault;
            
        }

        public NewsScreenViewModel()
        {
            string src = null;
            switch (NewsCategoryViewModel.SelectedCateGory)
            {
                case "WorldNews":
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () => true,
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            WorldNews = GetNewsModelList(output);
                            OnPropertyChanged("WorldNews");
                        }
                    }.Execute("http://www.thanhnien.com.vn/_layouts/newsrss.aspx?Channel=Th%E1%BA%BF+gi%E1%BB%9Bi+tr%E1%BA%BB");
                    break;

                case "PoliticNews":
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () => true,
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            PoliticNews = GetNewsModelList(output);
                            OnPropertyChanged("PoliticNews");
                        }
                    }.Execute("http://www.thanhnien.com.vn/_layouts/newsrss.aspx?Channel=Ch%C3%ADnh+tr%E1%BB%8B+-+X%C3%A3+h%E1%BB%99i");
                    break;

                case "HealthNews":
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () => true,
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            HealthNews = GetNewsModelList(output);
                            OnPropertyChanged("HealthNews");
                        }
                    }.Execute("http://www.thanhnien.com.vn/_layouts/newsrss.aspx?Channel=S%E1%BB%A9c+kh%E1%BB%8Fe");
                    break;

                case "ScienceNews":
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () => true,
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            ScienceNews = GetNewsModelList(output);
                            OnPropertyChanged("ScienceNews");
                        }
                    }.Execute("http://www.thanhnien.com.vn/_layouts/newsrss.aspx?Channel=Khoa+h%E1%BB%8Dc");
                    break;

                case "TechNews":
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () => true,
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            TechNews = GetNewsModelList(output);
                            OnPropertyChanged("TechNews");
                        }
                    }.Execute("http://www.thanhnien.com.vn/_layouts/newsrss.aspx?Channel=C%C3%B4ng+ngh%E1%BB%87+th%C3%B4ng+tin");
                    break;
            }
            
        }

        public List<NewsModel> WorldNews { get; private set; }
        public List<NewsModel> PoliticNews { get; private set; }
        public List<NewsModel> HealthNews { get; private set; }
        public List<NewsModel> ScienceNews { get; private set; }
        public List<NewsModel> TechNews { get; private set; }
 
        public string ContentViewer { get; private set; }

        private static string CustomTrimming(string text)
        {
            if (text == null) return text;

            int i = 0;
            while (i < text.Length)
            {
                if (text[0] == ' ')
                    text = text.Remove(0, 1);
                else if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    text = text.Remove(i, 2);
                else if (text[i] == ' ' && i + 1 < text.Length && text[i + 1] == ' ')
                    text = text.Remove(i, 1);
                else if (text[i] == ' ' && i + 1 == text.Length - 1)
                    text = text.Remove(i, 1);
                else i++;
            }

            return text;
        }

        private List<NewsModel> GetNewsModelList(string src)
        {
            var result = new List<NewsModel>();

            /*
            const string NewsModelStart = "<NewsModel id=";
            const string NewsModelEnd = "</NewsModel>";
            const string titleStart = "<h2 class=\"entry-title\">";
            const string urlStart = "<a href=\"";
            const string SummaryStart = "<div class=\"entry-Content group\">";
            const string imageStart = "<figure";

            string NewsModelUrl;
            string title;
            string Summary;
            string imageUrl;

            int iBegin = 0;
            int iEnd = 0;
            int iNewsModelEnd = 0;

            while ((iBegin = src.IndexOf(NewsModelStart, iBegin, StringComparison.Ordinal)) > 0)
            {
                iNewsModelEnd = src.IndexOf(NewsModelEnd, iBegin, StringComparison.Ordinal);

                // Get Image Url (if there is)
                int iBeginTemp = src.IndexOf(imageStart, iBegin, StringComparison.Ordinal);
                if (iBeginTemp != -1 && iBeginTemp < iNewsModelEnd)
                {
                    iBegin = iBeginTemp;
                    const string imageUrlStart = "<img src=\"";
                    iBegin = src.IndexOf(imageUrlStart, iBegin, StringComparison.Ordinal);
                    iBegin += imageUrlStart.Length;
                    iEnd = src.IndexOf("\"", iBegin, StringComparison.Ordinal);
                    imageUrl = src.Substring(iBegin, iEnd - iBegin);
                    iBegin = iEnd;
                }
                else
                {
                    imageUrl = null;
                }

                // Get NewsModel Url
                iBegin = src.IndexOf(titleStart, iBegin, StringComparison.Ordinal);
                iBegin = src.IndexOf(urlStart, iBegin, StringComparison.Ordinal);
                iBegin += urlStart.Length;
                iEnd = src.IndexOf("\"", iBegin, StringComparison.OrdinalIgnoreCase);
                NewsModelUrl = src.Substring(iBegin, iEnd - iBegin);
                iBegin = iEnd;

                // Get NewsModel Title
                iBegin = src.IndexOf(">", iEnd, StringComparison.Ordinal);
                iBegin += 1;
                iEnd = src.IndexOf("</a>", iBegin, StringComparison.Ordinal);
                title = src.Substring(iBegin, iEnd - iBegin);
                iBegin = iEnd;

                // Get NewsModel Summary
                iBegin = src.IndexOf(SummaryStart, iBegin, StringComparison.Ordinal);
                iBegin = src.IndexOf("<p>", iBegin, StringComparison.Ordinal);
                iBegin += 3;
                iEnd = src.IndexOf("</p>", iBegin, StringComparison.Ordinal);
                Summary = src.Substring(iBegin, iEnd - iBegin);
                iBegin = iEnd;

                result.Add(new NewsModel
                {
                    Title = title,
                    Url = NewsModelUrl,
                    ImageUrl = imageUrl,
                    Summary = Summary
                });
            }
            */

            //This is to decode the html special character into UTF-8 unicode
            src = WebUtility.HtmlDecode(src);

            //doc contains the structure of html document
            var doc = new HtmlDocument();
            doc.LoadHtml(src);

            //Get the titles and the urls. They are between the <item> tags
            HtmlNodeCollection divnodes = doc.DocumentNode.SelectNodes("//item");
            if (divnodes == null) return null;

            foreach (HtmlNode divnode in divnodes)
            {
                var element = new NewsModel
                {
                    Title = divnode.ChildNodes["title"].InnerText,
                    Url =
                        divnode.OuterHtml.Substring(
                            divnode.OuterHtml.IndexOf("<link>", StringComparison.Ordinal) + 6,
                            divnode.OuterHtml.IndexOf("<description>", StringComparison.Ordinal) -
                            (divnode.OuterHtml.IndexOf("<link>", StringComparison.Ordinal) + 6)
                            )
                };

                //BUG, </link> tag missing
                //element.url = divnode.ChildNodes["link"].InnerText;
                //can't use this to get the url out =.=

                //Resort to this solution to get the url
                element.Url = CustomTrimming(element.Url);
                element.Date = element.Url.Substring(element.Url.IndexOf("Pages/", StringComparison.Ordinal) + 6, 8);
                string day = element.Date.Substring(6, 2);
                element.Date = element.Date.Remove(6, 2);
                element.Date = element.Date.Insert(6, "/" + element.Date.Substring(0, 4));
                element.Date = element.Date.Remove(0, 4);
                element.Date = element.Date.Insert(0, day + "/");

                element.Summary = divnode.ChildNodes["description"].InnerText;
                element.Summary = element.Summary.Remove(0, 14);
                element.Summary = element.Summary.Remove(element.Summary.IndexOf("]]>", StringComparison.Ordinal), 3);
                result.Add(element);
            }
            return result;
        }

        /* Get NewsModel Content */
        public static NewsModel GetNewsModelContent(String source)
        {
            var result = new NewsModel();
            /*
            result.relatedNewsModel = new List<NewsModel>();
            result.images = new List<Image>();
            */
            //This is to decode the html special character into UTF-8 unicode
            source = WebUtility.HtmlDecode(source);

            //doc contains the structure of html document
            var doc = new HtmlDocument();
            doc.LoadHtml(source);

            //Get title. It is located between <h1 class='mainTitle'>...

            if (doc.DocumentNode.SelectSingleNode("//h1[@class='mainTitle']") != null)
            {
                result.Title = doc.DocumentNode.SelectSingleNode("//h1[@class='mainTitle']")
                    .InnerText;
                result.Title = CustomTrimming(result.Title);
            }

            //Get date of publish. It is located between <span class='date-line'>...
            if (doc.DocumentNode.SelectSingleNode("//span[@class='date-line']") != null)
            {
                result.Date = doc.DocumentNode.SelectSingleNode("//span[@class='date-line']")
                    .InnerText;
                result.Date = CustomTrimming(result.Date);
            }

            //EVERYTHING BELOWS ARE LOCATED BETWEEN <div class='NewsModel-Content NewsModel-Content'>...

            //Get author name. It is located between <p style='text-align:right'>...
            if (
                doc.DocumentNode.SelectSingleNode(
                    "//div[@class='article-content article-content03']/p[@style='text-align:right']") != null)
            {
                result.AuthorName =
                    doc.DocumentNode.SelectSingleNode(
                        "//div[@class='article-content article-content03']/p[@style='text-align:right']")
                        .InnerText;
                result.AuthorName = CustomTrimming(result.AuthorName);
            }

            //Get succinct Content. It is located between <h2 class='ms-rteElement-H2'>...
            if (
                doc.DocumentNode.SelectSingleNode(
                    "//div[@class='NewsModel-Content NewsModel-Content03']/h2[@class='ms-rteElement-H2']") != null)
            {
                result.Content +=
                    doc.DocumentNode.SelectSingleNode(
                        "//div[@class='NewsModel-Content NewsModel-Content03']/h2[@class='ms-rteElement-H2']")
                        .InnerText;
                result.Content = CustomTrimming(result.Content);
                result.Content += "\r\n";
            }

            /*
            //Get Related NewsModels. They are located between the tags <a> between the last <p> tag
            var related_divnodes = doc.DocumentNode.SelectNodes("//div[@class='NewsModel-Content NewsModel-Content03']//p[last()]/a");
            if (related_divnodes != null)
            {
                foreach (var item in related_divnodes)
                {
                    if (item.Attributes["href"] != null)
                    {
                        NewsModel new_related_NewsModel = new NewsModel();
                        new_related_NewsModel.title = item.InnerText;
                        new_related_NewsModel.url = "http://www.thanhnien.com.vn" + item.Attributes["href"].Value;

                        result.relatedNewsModel.Add(new_related_NewsModel);
                    }
                }
            }
            */
            //Get Actual Contents. They are located between the <p> tags (excluded the last <p> tag, 
            //which is where the related NewsModels are located)
            HtmlNodeCollection contentparagraphDivnodes =
                doc.DocumentNode.SelectNodes(
                    "//div[@class='article-content article-content03']/p[position()<last()]");
            if (contentparagraphDivnodes != null)
            {
                foreach (HtmlNode item in contentparagraphDivnodes)
                {
                    //Check if there are some related NewsModels missing in this block 
                    //(Usually located on the first/second paragraph)
                    if (item.ChildNodes.Count != 0)
                    {
                        //Checking instance
                        bool check = false;

                        foreach (HtmlNode child in item.ChildNodes)
                        {
                            //Checking if the block is a consists of all <a> tags or not
                            if (child.Name != "a" && child.Name != "br" && child.InnerText != "\r\n" &&
                                child.InnerText != " ")
                            {
                                check = true;
                                break;
                            }
                        }
                        /*
                        //If so then this suggests this block contains the related NewsModels
                        if (!check)
                        {
                            //Proceed to getting the related NewsModels
                            foreach (var item_ in item.ChildNodes)
                            {
                                if (item_.Name == "a" && item_.Attributes["href"] != null)
                                {
                                    var newRelatedNewsModel = new NewsModel();
                                    newRelatedNewsModel.Title = item_.InnerText;
                                    newRelatedNewsModel.Url = "www.thanhnien.com.vn" + item_.Attributes["href"].Value;

                                    result.relatedNewsModel.Add(newRelatedNewsModel);
                                }
                            }

                            //Get next item, no need to make this block an NewsModel Content
                            continue;
                        }
                        */
                    }

                    //This block belongs to the actual Content. Add this paragraph into the Contents
                    if (item.Attributes.Count == 0)
                    {
                        string newContent = item.InnerText;
                        newContent = CustomTrimming(newContent);
                        newContent += "\r\n";

                        if (newContent != "")
                            result.Content += newContent;
                    }
                }
            }

            /*
            //Extracting images descriptions and urls. 
            //They are located between <table><tbody><tr><td><p> tags (EXCEPTION?) 
            var imgbox_divnodes = doc.DocumentNode.SelectNodes("//div[@class='NewsModel-Content NewsModel-Content03']//table//tbody//tr//td//p");
            if (imgbox_divnodes != null)
            {
                foreach (var item in imgbox_divnodes)
                {
                    Image newimage = new Image();

                    //var pt = item;

                    //The addresses of the descriptions are varied on each NewsModel pages
                    //Normally they should be in /table/tbody/tr/td/p/span or /table/tbody/tr/td/p/
                    //Please check if there is any exception to apply change
                    //This should work fine, if it doesn't find any description or src, 
                    //the string should yield null reference (PENDING FOR TEST....)
                    if (item.Element("span") != null && item.Element("span").InnerText != "")
                        newimage.description = item.Element("span").InnerText;
                    else
                        newimage.description = item.InnerText;

                    //The same is applied to the image urls
                    //They should be in /table/tbody/tr/td/p/span/img or /table/tbody/tr/td/p/img
                    //Exceptions verifying (PENDING FOR TEST...)
                    if (item.Element("span") != null &&
                        item.Element("span").Element("img") != null)
                    {
                        newimage.url = "www.thanhnien.com.vn" +
                                       item.Element("span").Element("img").Attributes["src"].Value;
                    }
                    else if (item.Element("img") != null)
                    {
                        newimage.url = "www.thanhnien.com.vn" +
                                       item.Element("img").Attributes["src"].Value;
                    }

                    if (newimage.url != null)
                        result.images.Add(newimage);
                }
                
            }
            */
            return result;
        }

    }
}