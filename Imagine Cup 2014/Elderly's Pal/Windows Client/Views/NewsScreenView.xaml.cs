using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Kinect.Toolkit.Controls;
using mshtml;
using NiceDreamers.Windows.Models;
using NiceDreamers.Windows.ViewModels;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace NiceDreamers.Windows.Views
{
    /// <summary>
    ///     Interaction logic for NewsScreenView.xaml
    /// </summary>
    public partial class NewsScreenView : UserControl
    {
        private WebBrowser wb;

        public NewsScreenView()
        {
            InitializeComponent();
            DataContext = new NewsScreenViewModel();
            ((NewsScreenViewModel) DataContext).PropertyChanged += NewsScreenViewModel_PropertyChanged;
            //NewsScreenViewModel.PropertyChanged += NewsScreenViewModel_PropertyChanged;
            //this.WebBrowserContentViewer.ScriptErrorsSuppressed = true;
        }

        private void NewsScreenViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemsControlNews.ItemsSource = null;
            switch (e.PropertyName)
            {
                case "WorldNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).WorldNews;
                    break;

                case "PoliticNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).PoliticNews;
                    break;

                case "HealthNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).HealthNews;
                    break;

                case "ScienceNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).ScienceNews;
                    break;

                case "TechNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).TechNews;
                    break;
            }
        }

        private void ItemsControl_ItemClicked(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource as KinectTileButton != null)
            {
                var selectedItem = (e.OriginalSource as KinectTileButton).DataContext as NewsModel;
                if (selectedItem != null)
                {
                    new AsyncTask<string, int, string>
                    {
                        onPreExecute = () =>
                        {
                            WebBrowserContentViewer.Visibility = Visibility.Hidden;
                            GridLoading.Visibility = Visibility.Visible;
                            (Resources["Loading"] as Storyboard).Begin();
                            return true;
                        },
                        onUpdate = progress => { },
                        doInBackground = (input, process) =>
                        {
                            string result = null;
                            result = HtmlDownloader.byWebClient(input, Encoding.UTF8);
                            return result;
                        },
                        onPostExecute = output =>
                        {
                            string itemContent = NewsScreenViewModel.AnalyzeArticle(output);
                            WebBrowserContentViewer.NavigateToString(itemContent);
                            (Resources["Loading"] as Storyboard).Stop();
                            WebBrowserContentViewer.Visibility = Visibility.Visible;
                            GridLoading.Visibility = Visibility.Hidden;
                        }
                    }.Execute(selectedItem.Url);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            switch (NewsCategoryViewModel.SelectedCateGory)
            {
                case "WorldNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).WorldNews;
                    break;

                case "PoliticNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).PoliticNews;
                    break;

                case "HealthNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).HealthNews;
                    break;

                case "ScienceNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).ScienceNews;
                    break;

                case "TechNews":
                    ItemsControlNews.ItemsSource = ((NewsScreenViewModel) DataContext).TechNews;
                    break;
            }
        }

        private void PageUpButtonClick(object sender, RoutedEventArgs e)
        {
            var htmlDoc = WebBrowserContentViewer.Document as HTMLDocument;
            if (htmlDoc != null) htmlDoc.parentWindow.scrollBy(0, -20);
        }

        private void PageDownButtonClick(object sender, RoutedEventArgs e)
        {
            var htmlDoc = WebBrowserContentViewer.Document as HTMLDocument;
            if (htmlDoc != null) htmlDoc.parentWindow.scrollBy(0, 20);
        }
    }
}