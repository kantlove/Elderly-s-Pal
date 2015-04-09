using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Collections.ObjectModel;
using HtmlAgilityPack;
using System.Windows;

namespace NiceDreamers.Windows.Models
{
    public class NewsItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
        private string _Name;
        private string _LinkUrl;
        private string _Source;        
        private string _Description;
        private string _Content;
        private string _DatePublished;
        private List<NewsItem> _RelatedItem = new List<NewsItem>();
        private string _shortContent = "";
        private string _category;
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
                NotifyPropertyChanged("Category");
            }
        }
        public string Name 
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public string LinkUrl
        {
            get
            {
                return _LinkUrl;
            }
            set
            {
                _LinkUrl = value;
                NotifyPropertyChanged("LinkUrl");
            }
        }

        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                NotifyPropertyChanged("Source");
            }
        }
        public string ContentText
        {
            get
            {
                return HtmlDownloader.removeHtml(Content);
            }
        }
        public ObservableCollection<ItemTag> OwnTagList = new ObservableCollection<ItemTag>();
        public ObservableCollection<ItemTag> TagList = new ObservableCollection<ItemTag>();
        private bool isOffline = false;
        public bool IsOffline
        {
            get
            {
                return isOffline;
            }
            set
            {
                isOffline = value;
                NotifyPropertyChanged("IsOffline");
                NotifyPropertyChanged("AllTag");
            }
        }
        public string AllTag
        {
            get
            {
                ObservableCollection<ItemTag> tagList = TagList;                
                if (tagList.Count == 0)
                    return "Không có từ khóa";
                string tmp = tagList[0].Title;
                for (int i=1; i<tagList.Count; ++i)
                {
                    tmp += ", " + tagList[i].Title;
                }
                return tmp;
            }
        }
        public void addToTagList(ItemTag tag)
        {
            TagList.Add(tag);
            NotifyPropertyChanged("TagList");
            NotifyPropertyChanged("AllTag");
        }
        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {             
                _Description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                _Content = value;
                NotifyPropertyChanged("Content");
            }
        }
        
        public string ShortContent
        {
            get { return _shortContent; }
            set
            {
                _shortContent = value;
                NotifyPropertyChanged("ShortContent");
                NotifyPropertyChanged("ModifiedShortContent");
            }
        }
        public string ModifiedShortContent
        {
            get 
            {
                //reduce shortcontent's length to fit in design
                if (ShortContent.Length > 140)
                    return ShortContent.Substring(0, 137) + "...";
                return ShortContent;
            }
        }
        public string DatePublished
        {
            get
            {
                return _DatePublished;
            }
            set
            {
                _DatePublished = value;
                NotifyPropertyChanged("DatePublished");
            }
        }

        public string DateStandard;
        private string _imageLink; //image poster (if have one)
        public string ImageLink
        {
            get { return _imageLink; }
            set
            {
                _imageLink = value;
                NotifyPropertyChanged("ImageLink");
            }
        }       
        public ObservableCollection<NewsItem> RelatedItem = new ObservableCollection<NewsItem>();
        public void destroyAll()
        {           
            RelatedItem.Clear();            
            RelatedItem = null;
        }            
        public async Task LoadContentFrom(string url)
        {
            if (Content != null && Content != "")
            {                
                return;
            }
            try
            {       
                if (Source == "Vnexpress")
                {
                    await VnexpressController.LoadContentFrom(this, url);
                }
                else if (Source == "Dantri")
                {
                    await DantriController.LoadContentFrom(this, url);
                }
                //NewsTagger.getOwnTagList(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The system got an error at LoadPageFrom function, with message:\n" + ex.Message);
                return;
            }
        }
    }
}
