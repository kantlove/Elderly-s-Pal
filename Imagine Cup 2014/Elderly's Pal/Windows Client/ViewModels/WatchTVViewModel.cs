using System.Collections.ObjectModel;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.WatchTV)]
    public class WatchTVViewModel : ViewModelBase
    {
        public ObservableCollection<TVChannelModel> channelList;

        private ObservableCollection<SuatChieuModel> lichChieulist;

        public WatchTVViewModel()
        {
            InitChannelList();
            LichChieuList = new ObservableCollection<SuatChieuModel>();
        }

        public ObservableCollection<TVChannelModel> ChannelList
        {
            get { return channelList; }
            set
            {
                channelList = value;
                OnPropertyChanged("ChannelList");
            }
        }

        public ObservableCollection<SuatChieuModel> LichChieuList
        {
            get { return lichChieulist; }
            set
            {
                lichChieulist = value;
                OnPropertyChanged("LichChieuList");
            }
        }

        private void InitChannelList()
        {
            ChannelList = new ObservableCollection<TVChannelModel>
            {
                new TVChannelModel
                {
                    Name = "VTV1",
                    DirectLink = "http://m32.megafun.vn/live.vs?c=vstv002&q=medium&type=tv&token=_2_t97EyMNipnJpuIuxCMA",
                    ImageUrl = "/Content/Picture/tvLogo/VTV1.png",
                    LinkLichChieu = "http://www.mytv.com.vn/module/ajax/ajax_get_schedule.php?channelId=1"
                },
                new TVChannelModel
                {
                    Name = "VTV2",
                    DirectLink = "http://123.29.74.83/live.vs?c=vstv017&q=high&token=XDUIcoFXBMW9H9bcAGcZzA",
                    ImageUrl = "/Content/Picture/tvLogo/VTV2.png",
                    LinkLichChieu = "http://www.mytv.com.vn/module/ajax/ajax_get_schedule.php?channelId=59"
                },
                new TVChannelModel
                {
                    Name = "VTV3",
                    DirectLink = "http://123.29.74.83/live.vs?c=vstv004&q=high&token=XDUIcoFXBMW9H9bcAGcZzA",
                    ImageUrl = "/Content/Picture/tvLogo/VTV3.png",
                    LinkLichChieu = "http://www.mytv.com.vn/module/ajax/ajax_get_schedule.php?channelId=2"
                },
                new TVChannelModel
                {
                    Name = "VTV4",
                    DirectLink = "http://123.29.74.83/live.vs?c=vstv035&q=high&token=XDUIcoFXBMW9H9bcAGcZzA",
                    ImageUrl = "/Content/Picture/tvLogo/VTV4.png",
                    LinkLichChieu = "http://www.mytv.com.vn/module/ajax/ajax_get_schedule.php?channelId=60"
                },
                new TVChannelModel
                {
                    Name = "VTV6",
                    DirectLink = "http://123.29.74.83/live.vs?c=vstv014&q=high&token=XDUIcoFXBMW9H9bcAGcZzA",
                    ImageUrl = "/Content/Picture/tvLogo/VTV6.png",
                    LinkLichChieu = "http://www.mytv.com.vn/module/ajax/ajax_get_schedule.php?channelId=52"
                }
            };
        }
    }
}