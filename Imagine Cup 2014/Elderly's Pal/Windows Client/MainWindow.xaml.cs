using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NiceDreamers.Windows.Utilities;
using NiceDreamers.Windows.ViewModels;

namespace NiceDreamers.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        private const int MinimumScreenWidth = 1366;
        private const int MinimumScreenHeight = 768;

        /// <summary>
        ///     Controller used as ViewModel for this window.
        /// </summary>
        private readonly KinectController controller;

        /// <summary>
        ///     Mouse movement detector.
        /// </summary>
        private readonly MouseMovementDetector movementDetector;

        public MainWindow(KinectController controller)
        {
            InitializeComponent();

            if (controller == null)
            {
                throw new ArgumentNullException("controller", Properties.Resources.KinectControllerInvalid);
            }

            this.controller = controller;

            controller.EngagedUserColor = (Color) Resources["EngagedUserColor"];
            controller.TrackedUserColor = (Color) Resources["TrackedUserColor"];
            controller.EngagedUserMessageBrush = (Brush) Resources["EngagedUserMessageBrush"];
            controller.TrackedUserMessageBrush = (Brush) Resources["TrackedUserMessageBrush"];

            kinectRegion.HandPointersUpdated +=
                (sender, args) => controller.OnHandPointersUpdated(kinectRegion.HandPointers);

            DataContext = controller;

            movementDetector = new MouseMovementDetector(this);
            movementDetector.IsMovingChanged += OnIsMouseMovingChanged;
            movementDetector.Start();
        }

        /// <summary>
        ///     Handles all key up events and closes the window if the Escape key is pressed
        /// </summary>
        /// <param name="e">The data for the key up event</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            if (Key.Escape == e.Key)
            {
                Close();
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        ///     Handles Window.Loaded event, and prompts user if screen resolution does not meet
        ///     minimal requirements.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // get the main screen size
            double height = SystemParameters.PrimaryScreenHeight;
            double width = SystemParameters.PrimaryScreenWidth;

            // if the main screen is less than 1920 x 1080 then warn the user it is not the optimal experience 
            if ((width < MinimumScreenWidth) || (height < MinimumScreenHeight))
            {
                MessageBoxResult continueResult = MessageBox.Show(
                    Properties.Resources.SuboptimalScreenResolutionMessage,
                    Properties.Resources.SuboptimalScreenResolutionTitle, MessageBoxButton.YesNo);
                if (continueResult == MessageBoxResult.No)
                {
                    Close();
                }
            }
        }

        /// <summary>
        ///     Handles MouseMovementDetector.IsMovingChanged event and shows/hides the window bezel,
        ///     as appropriate.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnIsMouseMovingChanged(object sender, EventArgs e)
        {
            WindowBezelHelper.UpdateBezel(this, movementDetector.IsMoving);
            controller.IsInEngagementOverrideMode = movementDetector.IsMoving;
        }
    }
}