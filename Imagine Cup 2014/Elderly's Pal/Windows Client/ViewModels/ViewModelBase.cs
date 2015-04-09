using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using NiceDreamers.Windows.Navigation;
using NiceDreamers.Windows.Utilities;

namespace NiceDreamers.Windows.ViewModels
{
    /// <summary>
    ///     Abstract base for all view models. Provides support for property change notifications,
    ///     managing the current visual state and setup/cleanup during navigation
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, INavigableContext
    {
        /// <summary>
        ///     Name of the normal visual state
        /// </summary>
        internal const string NormalVisualState = "Normal";

        internal const string NavigatedToVisualState = "NavigatedTo";

        internal const string NavigatedFromVisualState = "NavigatedFrom";

        private string currentVisualStateName;

        private DateTime enterTime;

        private bool isUserInteracting = true;

        /// <summary>
        ///     Gets the current visual state. Changes to this property cause
        ///     the PropertyChanged event to be signaled
        /// </summary>
        public string VisualStateName
        {
            get { return currentVisualStateName; }

            protected set
            {
                currentVisualStateName = value;
                OnPropertyChanged("VisualStateName");
            }
        }

        /// <summary>
        ///     Gets a Boolean indicating whether Kinect user is currently interacting with the view.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsUserInteracting
        {
            get { return isUserInteracting; }

            set
            {
                if (value != isUserInteracting)
                {
                    isUserInteracting = value;
                    OnPropertyChanged("IsUserInteracting");
                }
            }
        }

        /// <summary>
        ///     Imported reference to the navigation manager for the current composition context
        /// </summary>
        [Import]
        public virtual NavigationManager NavigationManager { get; protected set; }

        /// <summary>
        ///     Overridable method that initializes the view model according to the
        /// </summary>
        /// <param name="parameter">Uri to the initialization data</param>
        public virtual void Initialize(Uri parameter)
        {
        }

        /// <summary>
        ///     Overridable method that performs any setup when the view model is being navigated to
        /// </summary>
        public virtual void OnNavigatedTo()
        {
            VisualStateName = NavigatedToVisualState;
            enterTime = DateTime.UtcNow;
        }

        /// <summary>
        ///     Overridable method that performs any cleanup when the view model is being navigated away from
        /// </summary>
        public virtual void OnNavigatedFrom()
        {
            VisualStateName = NavigatedFromVisualState;

            string name = GetType().GetAttributeValue((ExportNavigableAttribute x) => x.NavigableContextName);
            if (!String.IsNullOrWhiteSpace(name))
            {
                TimeSpan duration = DateTime.UtcNow.Subtract(enterTime);
                TimeCounter timeCounter = TimeCounter.Instance;
                timeCounter[name] += (int) duration.TotalSeconds;
            }
        }

        /// <summary>
        ///     Event that is signaled when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Signals the PropertyChanged event with the given property name
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        /// <summary>
        ///     Debug only method that verifies that a property exists on this view model.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                throw new ArgumentException("Invalid property name: " + propertyName);
            }
        }
    }
}