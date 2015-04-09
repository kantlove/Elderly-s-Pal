using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Kinect.Toolkit;
using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.NewsCategory)]
    public class NewsCategoryViewModel : ViewModelBase
    {
        public ICommand GoToWorldNews { get; private set; }
        public ICommand GoToPoliticNews { get; private set; }

        public ICommand GoToHealthNews { get; private set; }

        public ICommand GoToScienceNews { get; private set; }

        public ICommand GoToTechNews { get; private set; }

        public static string SelectedCateGory = null;

        public NewsCategoryViewModel()
        {
            GoToWorldNews = new RelayCommand(() =>
            {
                SelectedCateGory = "WorldNews";
                NavigationManager.NavigateTo("NewsScreen", new Uri("/Views/NewsScreenView.xaml", UriKind.Relative));
            });

            GoToPoliticNews = new RelayCommand(() =>
            {
                SelectedCateGory = "PoliticNews";
                NavigationManager.NavigateTo("NewsScreen", new Uri("/Views/NewsScreenView.xaml", UriKind.Relative));
            });

            GoToHealthNews = new RelayCommand(() =>
            {
                SelectedCateGory = "HealthNews";
                NavigationManager.NavigateTo("NewsScreen", new Uri("/Views/NewsScreenView.xaml", UriKind.Relative));
            });

            GoToScienceNews = new RelayCommand(() =>
            {
                SelectedCateGory = "ScienceNews";
                NavigationManager.NavigateTo("NewsScreen", new Uri("/Views/NewsScreenView.xaml", UriKind.Relative));
            });

            GoToTechNews = new RelayCommand(() =>
            {
                SelectedCateGory = "TechNews";
                NavigationManager.NavigateTo("NewsScreen", new Uri("/Views/NewsScreenView.xaml", UriKind.Relative));
            });
        }
    }
}
