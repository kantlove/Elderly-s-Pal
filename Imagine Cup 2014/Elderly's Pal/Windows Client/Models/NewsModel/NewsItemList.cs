using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using HtmlAgilityPack;
namespace NiceDreamers.Windows.Models
{
    public class NewsItemList : ObservableCollection<NewsItem>
    {
        
        //delete all items
        public void ClearAllItems()
        {
            this.Clear();
        }
    }
}
