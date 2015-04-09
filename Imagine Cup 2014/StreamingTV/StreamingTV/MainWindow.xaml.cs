using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Interop;
using WPFMediaKit.DirectShow.MediaPlayers;

namespace StreamingTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// http://www.tv24.vn/LiveTV/29/vtv2.html
    /// http://112.197.2.155:1935/live/vtv2_2/playlist.m3u8
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Closing += OnClosing;
            this.Loaded += MainWindow_Loaded;
        }
        DispatcherTimer timer = new DispatcherTimer();

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
         /*   MediaBase a = new LocationMedia("rtmp://112.197.2.155/live//vtv2_3");
            myVlcControl.Media = a;
            myVlcControl.PlaybackMode = PlaybackModes.Default;           */
         //   Player.Source = new Uri("http://ss-hdvip.ssphim.com/Static/filmStore//9645/playlist.m3u8");      
            //http://ss-hdvip.ssphim.com/Static/filmStore//8064-Frozen2013/playlist.m3u8
            Player.MediaFailed += Player_MediaFailed;
            Player.PreferedPositionFormat = MediaPositionFormat.MediaTime;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            Player.Volume = 100;
            Player.MediaOpened += Player_MediaOpened;            
        }

        void Player_MediaOpened(object sender, RoutedEventArgs e)
        {            
            currentPosition = TimeSpan.FromSeconds(0);
            timer.Start();
        }


        private TimeSpan currentPosition;
        void timer_Tick(object sender, EventArgs e)
        {
            currentPosition = currentPosition.Add(TimeSpan.FromSeconds(1));
            CurrentPosition.Content = currentPosition.ToString("g");
            SliderProgress.Value = ((double) Player.MediaPosition/Player.MediaDuration)*SliderProgress.Maximum;
        }

        void Player_MediaFailed(object sender, WPFMediaKit.DirectShow.MediaPlayers.MediaFailedEventArgs e)
        {
            MessageBox.Show(e.Exception + "\n" + e.Message);
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
        }

        private void SliderProgress_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void SliderProgress_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            
        }

        private void SliderProgress_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        
        }

        private void SliderProgress_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider varSlider = (Slider)sender;
            Player.MediaPosition = (long)((varSlider.Value / varSlider.Maximum) * Player.MediaDuration);
        }

        private void Player_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GridController.Visibility == Visibility.Collapsed)
                GridController.Visibility = Visibility.Visible;
            else GridController.Visibility = Visibility.Collapsed;
            
        }

        private void InputUrlTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.Key == Key.Enter)
            {
                var Input = (TextBox) sender;
                SliderProgress.Value = 0;
                timer.Stop();
                Player.Source = new Uri(Input.Text);                
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
        }
    }
}
