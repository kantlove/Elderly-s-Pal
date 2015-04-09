using NiceDreamers.Windows.Navigation;

namespace NiceDreamers.Windows.ViewModels
{
    [ExportNavigable(NavigableContextName = DefaultNavigableContexts.Picture)]
    public class HealthReportViewModel
    {
        private readonly TimeCounter timeCounter = TimeCounter.Instance;

        public int MusicScreenTime
        {
            get { return timeCounter[DefaultNavigableContexts.MusicScreen]; }
        }

        public int NewsScreenTime
        {
            get
            {
                int time = timeCounter[DefaultNavigableContexts.NewsScreen] +
                           timeCounter[DefaultNavigableContexts.NewsCategory];
                return time;
            }
        }

        public int PictureTime
        {
            get { return timeCounter[DefaultNavigableContexts.Picture]; }
        }

        public int WatchTV
        {
            get { return timeCounter[DefaultNavigableContexts.WatchTV]; }
        }
    }
}