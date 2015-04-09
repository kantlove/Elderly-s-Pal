using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xaml;
using Microsoft.Kinect.Toolkit;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.Navigation;
using NiceDreamers.Windows.Properties;
using NiceDreamers.Windows.Utilities;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.HomeScreen)]
    public class HomeScreenViewModel : ViewModelBase
    {
        /// <summary>
        ///     Path to the default model content resource
        /// </summary>
        internal const string DefaultHomeScreenModelContent = "Content/HomeScreen/HomeScreenContent.xaml";

        /// <summary>
        ///     Command that is executed when an experience option is selected
        /// </summary>
        private readonly RelayCommand<RoutedEventArgs> experienceSelected;

        /// <summary>
        ///     Initializes a new instance of the HomeScreenViewModel class and loads model content from the default resource path
        /// </summary>
        public HomeScreenViewModel()
            : this(PackUriHelper.CreatePackUri(DefaultHomeScreenModelContent))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the HomeScreenViewModel class that loads model content from the given Uri
        /// </summary>
        /// <param name="modelContentUri">Uri to the collection of AttractScreenImage models to be loaded</param>
        public HomeScreenViewModel(Uri modelContentUri)
        {
            experienceSelected = new RelayCommand<RoutedEventArgs>(OnExperienceSelected);

            var streamResourceInfo = Application.GetResourceStream(modelContentUri);
            if (streamResourceInfo != null)
                using (Stream experienceModelsStream = streamResourceInfo.Stream)
                {
                    var experiences = XamlServices.Load(experienceModelsStream) as IList<ExperienceOptionModel>;
                    if (null == experiences)
                    {
                        throw new InvalidDataException();
                    }

                    Experiences = new ObservableCollection<ExperienceOptionModel>(experiences);
                }
        }

        /// <summary>
        ///     Gets the experience selected command
        /// </summary>
        public ICommand ExperienceSelectedCommand
        {
            get { return experienceSelected; }
        }

        /// <summary>
        ///     Gets the observable collection of experiences selectable from the home screen
        /// </summary>
        public ObservableCollection<ExperienceOptionModel> Experiences { get; private set; }

        /// <summary>
        ///     Invoked when the ExperienceSelectedCommand is executed. Navigates to the selected experience
        /// </summary>
        private void OnExperienceSelected(RoutedEventArgs e)
        {
            var selected = ((ContentControl) e.OriginalSource).Content as ExperienceOptionModel;
            if (null == selected)
            {
                throw new InvalidOperationException(Resources.HomeScreenInvalidExperienceSelected);
            }

            if (null != selected.NavigableContextName)
            {
                NavigationManager.NavigateTo(selected.NavigableContextName, selected.NavigableContextParameter);
            }
        }
    }
}