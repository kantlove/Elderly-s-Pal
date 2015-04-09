using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.Picture)]
    public class PictureViewModel : ViewModelBase
    {
        public string Title { get { return "To Huu Quan"; } }

        public string FirstImage
        {
            get { return FakeSource.FirstOrDefault(); }
        }

        public List<string> FakeSource 
        { 
            get 
            { 
                return new List<string>()
                {
                    "/Content/Picture/pic.jpg",
                    "/Content/Picture/pic2.jpg",
                    "/Content/Picture/pic3.jpg",
                    "/Content/Picture/pic4.jpg",
                    "/Content/Picture/pic5.jpg",
                    "/Content/Picture/pic.jpg",
                    "/Content/Picture/pic2.jpg",
                    "/Content/Picture/pic3.jpg",
                    "/Content/Picture/pic4.jpg",
                    "/Content/Picture/pic5.jpg",
                    "/Content/Picture/pic.jpg",
                    "/Content/Picture/pic2.jpg",
                    "/Content/Picture/pic3.jpg",
                    "/Content/Picture/pic4.jpg",
                    "/Content/Picture/pic5.jpg",
                    "/Content/Picture/pic.jpg",
                    "/Content/Picture/pic2.jpg",
                    "/Content/Picture/pic3.jpg",
                    "/Content/Picture/pic4.jpg",
                    "/Content/Picture/pic5.jpg",
                    "/Content/Picture/pic.jpg",
                    "/Content/Picture/pic2.jpg",
                    "/Content/Picture/pic3.jpg",
                    "/Content/Picture/pic4.jpg",
                    "/Content/Picture/pic5.jpg",
                }; 
            } 
        }
    }
}
