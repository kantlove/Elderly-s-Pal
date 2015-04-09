using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NiceDreamers.Windows.Models
{
    public class TVChannelModel : INotifyPropertyChanged
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged("Name"); }
        }

        private string directLink;

        public string DirectLink
        {
            get { return directLink; }
            set { directLink = value; NotifyPropertyChanged("DirectLink"); }
        }

        private string linkLichChieu;

        public string LinkLichChieu
        {
            get { return linkLichChieu; }
            set { linkLichChieu = value; NotifyPropertyChanged("LinkLichChieu"); }
        }

        private string imageUrl;

        public string ImageUrl
        {
            get { return imageUrl; }
            set { imageUrl = value; NotifyPropertyChanged("ImageUrl"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
