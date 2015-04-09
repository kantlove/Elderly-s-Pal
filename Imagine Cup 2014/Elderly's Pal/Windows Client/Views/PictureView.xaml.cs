using Microsoft.Kinect.Toolkit.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceDreamers.Windows.Views
{
    /// <summary>
    /// Interaction logic for PictureView.xaml
    /// </summary>
    public partial class PictureView : UserControl
    {
        private readonly int PixelScrollByAmount = 20;

        public PictureView()
        {
            InitializeComponent();
            
        }

        private void PageLeftButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - PixelScrollByAmount);
        }

        private void PageRightButtonClick(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + PixelScrollByAmount);
        }

        private void PictureList_ItemClick(object sender, RoutedEventArgs e)
        {
            var kinectTileButton = e.OriginalSource as KinectTileButton;
            if (kinectTileButton != null)
            {
                string source = (string)kinectTileButton.DataContext;
                GridSelectedImage.Visibility = System.Windows.Visibility.Visible;
                var storyboard = Resources["ShowImage"] as Storyboard;
                if (storyboard != null) storyboard.Begin();
                ImageSelected.Source = new BitmapImage(new Uri(source, UriKind.RelativeOrAbsolute));
            }
        }

        private void ButtonBack_MouseEnter(object sender, MouseEventArgs e)
        {
            var kinectTileButton = sender as KinectTileButton;
            if (kinectTileButton != null) kinectTileButton.Opacity = 1;
        }

        private void ButtonBack_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as KinectTileButton).Opacity = 0.3;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            (sender as KinectTileButton).Opacity = 0.3;
            
            GridSelectedImage.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
