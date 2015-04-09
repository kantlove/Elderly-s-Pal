using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceDreamers.Windows.Models
{
    public class Category : INotifyPropertyChanged
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
        private string _title = "";
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        private string _link;
        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                NotifyPropertyChanged("Link");
            }
        }
        public string getLinkFromPage(int pageIndex, string source)
        {
            if (source == "Vnexpress")
            {
                return Link + "/page/" + pageIndex + ".html";
            }
            else if (source == "Dantri")
            {
                return Link + "/trang-" + pageIndex + ".htm";
            }
            return null;
        }
    }
}
