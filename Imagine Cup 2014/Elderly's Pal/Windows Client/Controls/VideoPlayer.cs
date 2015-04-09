using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NiceDreamers.Windows.Controls
{
    /// <summary>
    ///     A video player which automatically loops and can be controlled through a single Boolean dependency property.
    /// </summary>
    [TemplatePart(Name = MediaElementSitePartName, Type = typeof (MediaElement))]
    [TemplatePart(Name = VideoProgressBarSitePartName, Type = typeof (MediaElement))]
    [TemplatePart(Name = DurationSitePartName, Type = typeof (MediaElement))]
    [TemplatePart(Name = CurrentProgressSitePartName, Type = typeof (MediaElement))]
    public class VideoPlayer : Control
    {
        /// <summary>
        ///     Template part name for the MediaElement responsible for displaying the video.
        /// </summary>
        private const string MediaElementSitePartName = "MediaElementSite";

        /// <summary>
        ///     Template part name for the ProgressBar responsible for displaying the playback progress.
        /// </summary>
        private const string VideoProgressBarSitePartName = "VideoProgressBarSite";

        /// <summary>
        ///     Template part name for the TextBlock responsible for displaying the duration of the video.
        /// </summary>
        private const string DurationSitePartName = "DurationSite";

        /// <summary>
        ///     Template part name for the TextBlock responsible for displaying the current progress of the video.
        /// </summary>
        private const string CurrentProgressSitePartName = "CurrentProgressSite";

        public static readonly DependencyProperty IsPlayingProperty =
            DependencyProperty.Register("IsPlaying", typeof (bool), typeof (VideoPlayer),
                new UIPropertyMetadata(false, OnIsPlayingChanged));

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof (Uri), typeof (VideoPlayer),
                new UIPropertyMetadata(null, OnSourceChanged));

        public static readonly DependencyProperty ShowProgressBarProperty = DependencyProperty.Register(
            "ShowProgressBar", typeof (bool), typeof (VideoPlayer), new PropertyMetadata(true));

        private readonly DispatcherTimer progressTimer;

        private TextBlock currentProgressTextBlock;
        private TextBlock durationTextBlock;

        /// <summary>
        ///     MediaElement responsible for displaying the video.
        /// </summary>
        private MediaElement mediaElement;

        private ProgressBar progressBar;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline",
            Justification = "DefaultStyleKey.OverrideMetadata must be called from a static constructor")]
        static VideoPlayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (VideoPlayer),
                new FrameworkPropertyMetadata(typeof (VideoPlayer)));
        }

        public VideoPlayer()
        {
            progressTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(100.0)};
            progressTimer.Tick += OnProgressUpdateTick;
        }

        /// <summary>
        ///     Gets or sets whether the video is currently playing or is paused
        /// </summary>
        public bool IsPlaying
        {
            get { return (bool) GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }

        /// <summary>
        ///     Gets or sets whether the video is currently playing or is paused
        /// </summary>
        public bool ShowProgressBar
        {
            get { return (bool) GetValue(ShowProgressBarProperty); }
            set { SetValue(ShowProgressBarProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the Uri pointing to the video. This cannot be a resource Uri.
        /// </summary>
        public Uri Source
        {
            get { return (Uri) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        ///     Event that is signaled when the end of the video is reached.
        /// </summary>
        public event EventHandler VideoEnded;

        public override void OnApplyTemplate()
        {
            if (null != mediaElement)
            {
                mediaElement.MediaOpened -= OnVideoOpened;
                mediaElement.MediaEnded -= OnVideoEnded;
            }

            base.OnApplyTemplate();

            mediaElement = GetTemplateChild(MediaElementSitePartName) as MediaElement;
            progressBar = GetTemplateChild(VideoProgressBarSitePartName) as ProgressBar;
            durationTextBlock = GetTemplateChild(DurationSitePartName) as TextBlock;
            currentProgressTextBlock = GetTemplateChild(CurrentProgressSitePartName) as TextBlock;

            if (null != mediaElement)
            {
                mediaElement.MediaEnded += OnVideoEnded;
                mediaElement.MediaOpened += OnVideoOpened;
            }
        }

        private static void OnSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var videoPlayer = obj as VideoPlayer;
            if (videoPlayer != null && null != videoPlayer.mediaElement)
            {
                videoPlayer.mediaElement.Source = (Uri) e.NewValue;
            }
        }

        private static void OnIsPlayingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var videoPlayer = obj as VideoPlayer;
            if (videoPlayer != null && null != videoPlayer.mediaElement)
            {
                if (e.NewValue is bool && (bool) e.NewValue)
                {
                    videoPlayer.progressTimer.Start();
                    videoPlayer.mediaElement.Play();
                }
                else
                {
                    videoPlayer.progressTimer.Stop();
                    videoPlayer.mediaElement.Pause();
                }
            }
        }

        /// <summary>
        ///     Internal event handler that resets the position of the video to the start and invokes the VideoEnded event
        /// </summary>
        private void OnVideoEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;

            EventHandler handler = VideoEnded;
            if (null != handler)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnVideoOpened(object sender, RoutedEventArgs e)
        {
            if (null != progressBar)
            {
                progressBar.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            }

            if (null != durationTextBlock)
            {
                durationTextBlock.Text = mediaElement.NaturalDuration.TimeSpan.ToString(@"m\:ss",
                    CultureInfo.InvariantCulture);
            }
        }

        private void OnProgressUpdateTick(object sender, EventArgs e)
        {
            if (null != progressBar)
            {
                progressBar.Value = mediaElement.Position.TotalMilliseconds;
            }

            if (null != currentProgressTextBlock)
            {
                currentProgressTextBlock.Text = mediaElement.Position.ToString(@"m\:ss", CultureInfo.InvariantCulture);
            }
        }
    }
}