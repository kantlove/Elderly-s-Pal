using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Threading;
using System.Xaml;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.Navigation;
using NiceDreamers.Windows.Utilities;

namespace NiceDreamers.Windows.ViewModels
{
    /// <summary>
    ///     View model for the attract screen responsible for managing a collection of images.
    ///     The collection of images is iterated through (wrapping back to the beginning),
    ///     exposing each in turn as the current image to the view.
    /// </summary>
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.AttractScreen)]
    public class AttractScreenViewModel : ViewModelBase
    {
        /// <summary>
        ///     Path to the default model content resource
        /// </summary>
        internal const string DefaultAttractScreenModelContent = "Content/AttractScreen/AttractScreenContent.xaml";

        /// <summary>
        ///     Interval between exposing a new current image in milliseconds
        /// </summary>
        internal const double TimerIntervalMilliseconds = 3000;

        private readonly DispatcherTimer tickTimer;

        private Image currentImage;
        private int currentIndex;
        private ObservableCollection<Image> images;

        /// <summary>
        ///     Initializes a new instance of the AttractScreenViewModel class and loads model content from the default resource
        ///     path
        /// </summary>
        public AttractScreenViewModel()
            : this(PackUriHelper.CreatePackUri(DefaultAttractScreenModelContent))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the AttractScreenViewModel class that loads model content from the given Uri
        /// </summary>
        /// <param name="modelContentUri">Uri to the collection of AttractScreenImage models to be loaded</param>
        public AttractScreenViewModel(Uri modelContentUri)
        {
            LoadModels(modelContentUri);

            tickTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds)};
            tickTimer.Tick += (s, e) =>
            {
                currentIndex = currentIndex < Images.Count - 1 ? ++currentIndex : 0;
                CurrentImage = Images[currentIndex];
            };
        }

        /// <summary>
        ///     Gets the observable collection of all images.
        /// </summary>
        public ObservableCollection<Image> Images
        {
            get { return images; }
        }

        /// <summary>
        ///     Gets the current image to display on the attract screen. Changes to this property
        ///     cause the PropertyChanged event to be signaled
        /// </summary>
        public Image CurrentImage
        {
            get { return currentImage; }

            protected set
            {
                currentImage = value;
                OnPropertyChanged("CurrentImage");
            }
        }

        /// <summary>
        ///     Resets the current image to the first in the collection and starts the timer that
        ///     iterates through the collection. Called when this view model is navigated to.
        /// </summary>
        public override void OnNavigatedTo()
        {
            base.OnNavigatedTo();

            if (0 < Images.Count)
            {
                currentIndex = 0;
                CurrentImage = Images[currentIndex];
                tickTimer.Start();
            }
        }

        /// <summary>
        ///     Stops the timer iterating through the image collection.
        ///     Called when this view model is navigated away from.
        /// </summary>
        public override void OnNavigatedFrom()
        {
            base.OnNavigatedFrom();

            tickTimer.Stop();
        }

        /// <summary>
        ///     Loads the collection of AttractScreenImage models and transforms them to a collection of images.
        /// </summary>
        /// <param name="modelContentUri">Uri to the collection of AttractScreenImage models to be loaded</param>
        protected void LoadModels(Uri modelContentUri)
        {
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(modelContentUri);
            if (streamResourceInfo != null)
                using (Stream attractModelsStream = streamResourceInfo.Stream)
                {
                    images = new ObservableCollection<Image>(
                        from imageModel in XamlServices.Load(attractModelsStream) as IList<AttractImageModel>
                        select new Image {Source = new BitmapImage(imageModel.ImageUri)});
                }
        }
    }
}