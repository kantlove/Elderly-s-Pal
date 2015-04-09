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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect.Toolkit.Controls;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.ViewModels;

namespace NiceDreamers.Windows.Views
{
    /// <summary>
    /// Interaction logic for MusicScreenView.xaml
    /// </summary>
    public partial class MusicScreenView : UserControl
    {
        private readonly MediaPlayer musicPlayer = new MediaPlayer();
        private DispatcherTimer timer;

        public MusicScreenView()
        {
            InitializeComponent();
            musicPlayer.MediaFailed += (o, args) =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                MessageBox.Show("Media Failed!!");
            };
            musicPlayer.MediaOpened += new EventHandler(Media_Opened);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(Timer_Tick);
            //musicPlayer.LoadedBehavior = MediaState.Manual;
        }

        private void ItemsControlSongList_ItemClick(object sender, RoutedEventArgs e)
        {
            musicPlayer.Stop();
            timer.Stop();
            if (e.OriginalSource as KinectTileButton != null)
            {
                var selectedItem = (e.OriginalSource as KinectTileButton).DataContext as MusicModel;
                if (selectedItem != null)
                {
                    musicPlayer.Open(selectedItem.SongUri);
                    musicPlayer.Play();

                    TextblockSongName.Text = selectedItem.SongName;
                    ButtonPause.Visibility = Visibility.Visible;
                    ButtonPlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Media_Opened(object sender, EventArgs e)
        {
            if (musicPlayer.NaturalDuration.HasTimeSpan)
            {
                TimeSpan ts = musicPlayer.NaturalDuration.TimeSpan;
                this.TextblockTimer.Text = ts.ToString(@"mm\:ss");

                ProgressBarSeekBar.SmallChange = 1;
                ProgressBarSeekBar.Maximum = ts.TotalSeconds;
            }
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TextblockTimer.Text = String.Format("{0} / {1}", 
                musicPlayer.Position.ToString(@"mm\:ss"), musicPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));

            ProgressBarSeekBar.Value = musicPlayer.Position.TotalSeconds;
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (musicPlayer.CanPause)
            {
                musicPlayer.Pause();
                ButtonPlay.Visibility = Visibility.Visible;
                ButtonPause.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if(musicPlayer.HasAudio)
            {
                musicPlayer.Play();
                ButtonPause.Visibility = Visibility.Visible;
                ButtonPlay.Visibility = Visibility.Collapsed;
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            musicPlayer.Stop();
        }

        private void ButtonPlay_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Screen_Unloaded(object sender, RoutedEventArgs e)
        {
            musicPlayer.Stop();
        }

    }
}
