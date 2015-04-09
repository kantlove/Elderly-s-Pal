using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Kinect.Toolkit;

namespace NiceDreamers.Windows.Navigation
{
    /// <summary>
    ///     Maintains the current navigation context and a stack of prior contexts
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NavigationManager : INotifyPropertyChanged
    {
        private readonly ICollection<Lazy<INavigableContext, IExportNavigableMetadata>> navigableContexts =
            new List<Lazy<INavigableContext, IExportNavigableMetadata>>();

        /// <summary>
        ///     Command that is executed to navigate back one navigation context
        /// </summary>
        private readonly RelayCommand navigateBackCommand;

        /// <summary>
        ///     Stack of prior navigation contexts
        /// </summary>
        private readonly Stack<INavigableContext> navigationStack = new Stack<INavigableContext>();

        /// <summary>
        ///     Current navigation context
        /// </summary>
        private INavigableContext currentNavigationContext;

        public NavigationManager()
        {
            navigateBackCommand = new RelayCommand(GoBack, () => CanGoBack);
        }

        /// <summary>
        ///     Gets the current navigation context
        /// </summary>
        public INavigableContext CurrentNavigationContext
        {
            get { return currentNavigationContext; }

            protected set
            {
                if (null == value)
                {
                    throw new ArgumentNullException("value");
                }

                // Setting a new context calls OnNavigatedFrom on the current context (if it exists) 
                // and OnNavigatedTo on the new context.
                if (null != currentNavigationContext)
                {
                    currentNavigationContext.OnNavigatedFrom();
                }

                currentNavigationContext = value;
                currentNavigationContext.OnNavigatedTo();
                OnPropertyChanged("CurrentNavigationContext");
            }
        }

        /// <summary>
        ///     Gets whether a back navigation is valid
        /// </summary>
        public bool CanGoBack
        {
            get { return 0 < navigationStack.Count; }
        }

        /// <summary>
        ///     Gets the navigate back one context command
        /// </summary>
        public ICommand NavigateBackCommand
        {
            get { return navigateBackCommand; }
        }

        /// <summary>
        ///     Imported collection of all navigable contexts within the MEF composition. This collection utilizes lazy loading
        ///     to delay instantiation of INavigables until they are utilized.
        /// </summary>
        [ImportMany]
        internal ICollection<Lazy<INavigableContext, IExportNavigableMetadata>> NavigableContexts
        {
            get { return navigableContexts; }
        }

        /// <summary>
        ///     Event that is signaled when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Navigates to the supplied context and pushes the current context onto the navigation stack.
        /// </summary>
        /// <param name="navigationContextName">Exported name of Navigation context to navigate to</param>
        public void NavigateTo(string navigationContextName)
        {
            NavigateTo(navigationContextName, null);
        }

        /// <summary>
        ///     Navigates to the supplied context parameterized by the supplied data parameter and pushes the current context onto the navigation stack.
        /// </summary>
        /// <param name="navigationContextName">Exported name of Navigation context to navigate to</param>
        /// <param name="parameter">Uri to the initialization data.</param>
        public void NavigateTo(string navigationContextName, Uri parameter)
        {
            if (null == navigationContextName)
            {
                throw new ArgumentNullException("navigationContextName");
            }

            if (null != CurrentNavigationContext)
            {
                navigationStack.Push(CurrentNavigationContext);
            }

            INavigableContext navContext = GetNavigableContext(navigationContextName);
            navContext.Initialize(parameter);
            CurrentNavigationContext = navContext;
        }

        /// <summary>
        ///     Navigates to the supplied context and clears the navigation stack.
        /// </summary>
        /// <param name="homeNavigationContextName">Exported name of Navigation context to navigate to</param>
        public void NavigateToHome(string homeNavigationContextName)
        {
            NavigateToHome(homeNavigationContextName, null);
        }

        /// <summary>
        ///     Navigates to the supplied context parameterized by the supplied data parameter and clears the navigation stack.
        /// </summary>
        /// <param name="homeNavigationContextName">Exported name of Navigation context to navigate to</param>
        /// <param name="parameter">Uri to the initialization data.</param>
        public void NavigateToHome(string homeNavigationContextName, Uri parameter)
        {
            navigationStack.Clear();

            INavigableContext navContext = GetNavigableContext(homeNavigationContextName);
            navContext.Initialize(parameter);
            CurrentNavigationContext = navContext;
        }

        /// <summary>
        ///     Navigates back to the prior navigation context
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack)
            {
                throw new InvalidOperationException("No element to navigate back to");
            }

            CurrentNavigationContext = navigationStack.Pop();
        }

        /// <summary>
        ///     Signals the PropertyChanged event with the given property name
        /// </summary>
        /// <param name="propertyName">Name of the property that changed</param>
        protected void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        private INavigableContext GetNavigableContext(string navigationContextName)
        {
            return NavigableContexts.Single(nc => nc.Metadata.NavigableContextName.Equals(navigationContextName)).Value;
        }

        /// <summary>
        ///     Debug only method that verifies that a property exists on this object.
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