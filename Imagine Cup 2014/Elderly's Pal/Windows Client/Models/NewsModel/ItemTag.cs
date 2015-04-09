using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace NiceDreamers.Windows.Models
{
    public class ItemTag : INotifyPropertyChanged
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
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }
        private int _count=0;
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                NotifyPropertyChanged("Count");
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

    }
}
