using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect.Toolkit.Controls;
using NiceDreamers.Windows.Models;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace NiceDreamers.Windows.Views
{
    /// <summary>
    /// Interaction logic for WatchTVView.xaml
    /// </summary>
    public partial class WatchTVView : UserControl
    {
        public WatchTVView()
        {
            InitializeComponent();
            GridControl.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Collapsed;
            Player.MediaFailed += Player_MediaFailed;
            Player.MediaOpened += Player_MediaOpened;
            Player.MouseMove += Player_MouseMove;
            BtnPlayPause.DataContext = "/Content/Picture/Media-Pause.png";
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += timer_Tick;            
        }

        void timer_Tick(object sender, EventArgs e)
        {
            GridControl.Visibility = Visibility.Collapsed;            
        }
        DispatcherTimer timer = new DispatcherTimer();
        void Player_MouseMove(object sender, MouseEventArgs e)
        {
            timer.Stop();
            GridControl.Visibility = Visibility.Visible;
            timer.Start();
        }

        void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            StopLoading();
        }

        void Player_MediaFailed(object sender, WPFMediaKit.DirectShow.MediaPlayers.MediaFailedEventArgs e)
        {
            
            
        }

        void StartLoading(TVChannelModel tvChannel)
        {
            LichChieu.Items.Clear();
            string text = "";
            using (WebClient wc = new WebClient())
            {
                text = wc.DownloadString(new Uri(tvChannel.LinkLichChieu, UriKind.RelativeOrAbsolute));
                text = Regex.Unescape(text);
            }
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(text);
            var allNodes = doc.DocumentNode.SelectNodes("/p");
            if (allNodes.Any())
            {
                foreach (var node in allNodes)
                {
                    SuatChieuModel sc = new SuatChieuModel();
                    sc.Time = node.SelectSingleNode(node.XPath + "/strong").InnerText;
                    sc.Name = node.InnerText.Replace(sc.Time, "");
                    LichChieu.Items.Add(sc);
                }
            }            
            LoadingGrid.Visibility = Visibility.Visible;
        }

        void StopLoading()
        {
            LoadingGrid.Visibility = Visibility.Collapsed;
        }
        private void ChannelList_OnItemClick(object sender, RoutedEventArgs e)
        {
            var kinectTileButton = e.OriginalSource as KinectTileButton;
            if (kinectTileButton != null)
            {
                var tvChannel = (TVChannelModel)kinectTileButton.DataContext;
                if (Player.Source == null || !Player.Source.AbsoluteUri.Contains(tvChannel.DirectLink))
                {
                    StartLoading(tvChannel);
                    Player.Source = new Uri(tvChannel.DirectLink);
                }
            }
        }

        private void WatchTVView_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetFullScreenIcon();
        }

        private void Player_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Player_OnMouseEnter(object sender, MouseEventArgs e)
        {
            
        }

        private void BtnPlayPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (Player==null || Player.Source == null) return;
            if (Player.IsPlaying)
            {
                Player.Pause();
                BtnPlayPause.DataContext = "/Content/Picture/Media-Play.png";
            }
            else
            {
                Player.Play();
                BtnPlayPause.DataContext = "/Content/Picture/Media-Pause.png";
            }
        }

        private void BtnFullScreen_OnClick(object sender, RoutedEventArgs e)
        {
            var columSpan = Grid.GetColumnSpan(GridMedia);
            if (columSpan == 1)
            {
                Grid.SetColumnSpan(GridMedia, 2);
                Grid.SetRowSpan(GridTop, 2);
                BtnFullScreen.DataContext = "/Content/Picture/Full-Screen-Collapse.png";
            }
            else
            {
                Grid.SetColumnSpan(GridMedia, 1);
                Grid.SetRowSpan(GridTop, 1);
                BtnFullScreen.DataContext = "/Content/Picture/Full-Screen-Expand.png";
            }
        }

        private void SetFullScreenIcon()
        {
            var columSpan = Grid.GetColumnSpan(GridMedia);
            if (columSpan == 2)
            {
                BtnFullScreen.DataContext = "/Content/Picture/Full-Screen-Collapse.png";
            }
            else
            {
                BtnFullScreen.DataContext = "/Content/Picture/Full-Screen-Expand.png";
            }
        }
    }
}
