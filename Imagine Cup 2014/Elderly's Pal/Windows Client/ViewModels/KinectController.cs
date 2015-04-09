using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using NiceDreamers.Windows.Navigation;
using NiceDreamers.Windows.Properties;
using NiceDreamers.Windows.Utilities;

namespace NiceDreamers.Windows.ViewModels
{
    /// <summary>
    ///     Manages the lifetime of the Kinect sensor and calculates the current controlling user.
    /// </summary>
    [Export(typeof (KinectController))]
    public class KinectController : ViewModelBase
    {
        /// <summary>
        ///     Duration of time interval (in seconds) over which the state of the engagement
        ///     handoff prompts state remains in stasis after handoff is confirmed.
        /// </summary>
        private const double HandoffConfirmationStasisSeconds = 0.5;

        /// <summary>
        ///     Duration of time interval after which application navigates back to attract
        ///     screen when a user disengages when there IS another user to whom control
        ///     could be handed off.
        /// </summary>
        private readonly TimeSpan disengagementHandoffNavigationTimeout = TimeSpan.FromSeconds(10.0);

        /// <summary>
        ///     Timer used to keep track of time interval (in seconds) after which application
        ///     navigates back to attract screen when a user disengages.
        /// </summary>
        private readonly DispatcherTimer disengagementNavigationTimer = new DispatcherTimer();

        /// <summary>
        ///     Duration of time interval after which application navigates back to attract
        ///     screen when a user disengages when there are NO other users to whom control
        ///     could be handed off.
        /// </summary>
        private readonly TimeSpan disengagementNoHandoffNavigationTimeout = TimeSpan.FromDays(10.0);

        /// <summary>
        ///     Command that is executed when candidate user has confirmed intent to engage
        ///     when there is no engaged user present.
        /// </summary>
        private readonly RelayCommand<RoutedEventArgs> engagementConfirmationCommand;

        /// <summary>
        ///     Command that is executed when candidate user has confirmed intent to engage
        ///     when there is already an engaged user present.
        /// </summary>
        private readonly RelayCommand<RoutedEventArgs> engagementHandoffConfirmationCommand;

        /// <summary>
        ///     Component that keeps track of engagement state
        /// </summary>
        private readonly EngagementStateManager engagementStateManager = new EngagementStateManager();

        /// <summary>
        ///     Timer used to keep track of time interval over which the state of the engagement
        ///     handoff prompts state remains in stasis after handoff is confirmed.
        /// </summary>
        private readonly DispatcherTimer handoffConfirmationStasisTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(HandoffConfirmationStasisSeconds)
        };

        /// <summary>
        ///     Ids of users we choose to track.
        /// </summary>
        private readonly int[] recommendedUserTrackingIds = new int[2];

        /// <summary>
        ///     Component that manages finding a Kinect sensor
        /// </summary>
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();

        /// <summary>
        ///     Command that is executed on shutdown to cleanup
        /// </summary>
        private readonly RelayCommand shutdownCommand;

        /// <summary>
        ///     State of engagement confirmation prompt.
        /// </summary>
        private PromptState engagementConfirmationState = PromptState.Hidden;

        /// <summary>
        ///     True if engagement handoff barrier should be enabled (to prevent
        ///     candidate user from interacting with application UI), false otherwise.
        /// </summary>
        private bool isEngagementHandoffBarrierEnabled;

        /// <summary>
        ///     Boolean determining whether engagement state is currently being overridden.
        /// </summary>
        private bool isInEngagementOverrideMode;

        /// <summary>
        ///     Boolean determining whether any user is currently engaged or a candidate for engagement.
        /// </summary>
        private bool isUserActive;

        /// <summary>
        ///     Boolean determining whether any user is currently engaged.
        /// </summary>
        private bool isUserEngaged;

        /// <summary>
        ///     Boolean determining whether any user is currently a candidate for engagement.
        /// </summary>
        private bool isUserEngagementCandidate;

        /// <summary>
        ///     Boolean determining whether any user is currently being tracked.
        /// </summary>
        private bool isUserTracked;

        /// <summary>
        ///     State of handoff confirmation prompt for user on the left.
        /// </summary>
        private PromptState leftHandoffConfirmationState = PromptState.Hidden;

        /// <summary>
        ///     Handoff message background brush for user on the left.
        /// </summary>
        private Brush leftHandoffMessageBrush;

        /// <summary>
        ///     State of handoff message for user on the left.
        /// </summary>
        private PromptState leftHandoffMessageState = PromptState.Hidden;

        /// <summary>
        ///     Handoff message text for user on the left.
        /// </summary>
        private string leftHandoffMessageText;

        /// <summary>
        ///     State of handoff confirmation prompt for user on the right.
        /// </summary>
        private PromptState rightHandoffConfirmationState = PromptState.Hidden;

        /// <summary>
        ///     Handoff message background brush for user on the left.
        /// </summary>
        private Brush rightHandoffMessageBrush;

        /// <summary>
        ///     State of handoff message for user on the right.
        /// </summary>
        private PromptState rightHandoffMessageState = PromptState.Hidden;

        /// <summary>
        ///     Handoff message text for user on the right.
        /// </summary>
        private string rightHandoffMessageText;

        /// <summary>
        ///     Array of skeletons to process in each frame.
        /// </summary>
        private Skeleton[] skeletons;

        /// <summary>
        ///     State of start banner prompt.
        /// </summary>
        private PromptState startBannerState = PromptState.Hidden;

        public KinectController()
        {
            QueryPrimaryUserCallback = OnQueryPrimaryUserCallback;
            PreEngagementUserColors = new Dictionary<int, Color>();
            PostEngagementUserColors = new Dictionary<int, Color>();

            engagementStateManager.TrackedUsersChanged += OnEngagementManagerTrackedUsersChanged;
            engagementStateManager.CandidateUserChanged += OnEngagementManagerCandidateUserChanged;
            engagementStateManager.EngagedUserChanged += OnEngagementManagerEngagedUserChanged;
            engagementStateManager.PrimaryUserChanged += OnEngagementManagerPrimaryUserChanged;

            handoffConfirmationStasisTimer.Tick += OnHandoffConfirmationStasisTimerTick;
            disengagementNavigationTimer.Tick += OnDisengagementNavigationTick;

            shutdownCommand = new RelayCommand(Cleanup);
            engagementConfirmationCommand = new RelayCommand<RoutedEventArgs>(OnEngagementConfirmation);
            engagementHandoffConfirmationCommand = new RelayCommand<RoutedEventArgs>(OnEngagementHandoffConfirmation);

            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            sensorChooser.Start();
        }

        public override NavigationManager NavigationManager
        {
            get { return base.NavigationManager; }

            protected set
            {
                if (null != base.NavigationManager)
                {
                    base.NavigationManager.PropertyChanged -= OnNavigationManagerPropertyChanged;
                }

                base.NavigationManager = value;

                if (null != base.NavigationManager)
                {
                    base.NavigationManager.PropertyChanged += OnNavigationManagerPropertyChanged;
                }
            }
        }

        /// <summary>
        ///     Gets the KinectSensorChooser component
        /// </summary>
        public KinectSensorChooser KinectSensorChooser
        {
            get { return sensorChooser; }
        }

        /// <summary>
        ///     Gets the shutdown command
        /// </summary>
        public ICommand ShutdownCommand
        {
            get { return shutdownCommand; }
        }

        /// <summary>
        ///     Gets the command that is executed when candidate user has confirmed intent to engage
        ///     when there is no engaged user present.
        /// </summary>
        public ICommand EngagementConfirmationCommand
        {
            get { return engagementConfirmationCommand; }
        }

        /// <summary>
        ///     Gets the command that is executed when candidate user has confirmed intent to engage
        ///     when there is already an engaged user present.
        /// </summary>
        public ICommand EngagementHandoffConfirmationCommand
        {
            get { return engagementHandoffConfirmationCommand; }
        }

        /// <summary>
        ///     Callback that chooses who the primary user should be.
        /// </summary>
        public QueryPrimaryUserTrackingIdCallback QueryPrimaryUserCallback { get; private set; }

        /// <summary>
        ///     Gets whether engagement state is currently being overridden.
        /// </summary>
        public bool IsInEngagementOverrideMode
        {
            get { return isInEngagementOverrideMode; }

            set
            {
                isInEngagementOverrideMode = value;

                UpdateUserEngaged();
                UpdateUserTracked();
            }
        }

        /// <summary>
        ///     Gets whether any user is currently engaged with the application.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsUserEngaged
        {
            get { return isUserEngaged; }

            protected set
            {
                bool wasEngaged = isUserEngaged;

                isUserEngaged = value;
                OnPropertyChanged("IsUserEngaged");

                if (wasEngaged != isUserEngaged)
                {
                    PerformEngagementChangeNavigation();
                }

                UpdateCurrentNavigationContextState();
                UpdateUserActive();
                UpdateStartBannerState();
                UpdateEngagementHandoffBarrier();
            }
        }

        /// <summary>
        ///     Gets whether any user is currently an engagement candidate.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsUserEngagementCandidate
        {
            get { return isUserEngagementCandidate; }

            protected set
            {
                isUserEngagementCandidate = value;
                OnPropertyChanged("IsUserEngagementCandidate");

                UpdateUserActive();
                UpdateStartBannerState();
                UpdateEngagementConfirmationState();
                UpdateEngagementHandoffBarrier();
            }
        }

        /// <summary>
        ///     Gets whether any user is currently engaged with the application or is an
        ///     engagement candidate.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsUserActive
        {
            get { return isUserActive; }

            protected set
            {
                isUserActive = value;
                OnPropertyChanged("IsUserActive");
            }
        }

        /// <summary>
        ///     Gets whether any user is currently being tracked by the application.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsUserTracked
        {
            get { return isUserTracked; }

            protected set
            {
                isUserTracked = value;
                OnPropertyChanged("IsUserTracked");

                UpdateStartBannerState();
            }
        }

        /// <summary>
        ///     Gets the current state of start banner prompt.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState StartBannerState
        {
            get { return startBannerState; }

            protected set
            {
                startBannerState = value;
                OnPropertyChanged("StartBannerState");
            }
        }

        /// <summary>
        ///     Gets the current state of engagement confirmation prompt.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState EngagementConfirmationState
        {
            get { return engagementConfirmationState; }

            protected set
            {
                engagementConfirmationState = value;
                OnPropertyChanged("EngagementConfirmationState");
            }
        }

        /// <summary>
        ///     Gets whether the engagement handoff confirmation barrier should be enabled.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public bool IsEngagementHandoffBarrierEnabled
        {
            get { return isEngagementHandoffBarrierEnabled; }

            protected set
            {
                isEngagementHandoffBarrierEnabled = value;
                OnPropertyChanged("IsEngagementHandoffBarrierEnabled");
            }
        }

        /// <summary>
        ///     Gets the current state of handoff message for user on the left.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState LeftHandoffMessageState
        {
            get { return leftHandoffMessageState; }

            protected set
            {
                leftHandoffMessageState = value;
                OnPropertyChanged("LeftHandoffMessageState");
            }
        }

        /// <summary>
        ///     Gets the current handoff message text for user on the left.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public string LeftHandoffMessageText
        {
            get { return leftHandoffMessageText; }

            protected set
            {
                leftHandoffMessageText = value;
                OnPropertyChanged("LeftHandoffMessageText");
            }
        }

        /// <summary>
        ///     Gets the current handoff message background brush for user on the left.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public Brush LeftHandoffMessageBrush
        {
            get { return leftHandoffMessageBrush; }

            protected set
            {
                leftHandoffMessageBrush = value;
                OnPropertyChanged("LeftHandoffMessageBrush");
            }
        }

        /// <summary>
        ///     Gets the current state of handoff confirmation prompt for user on the left.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState LeftHandoffConfirmationState
        {
            get { return leftHandoffConfirmationState; }

            protected set
            {
                leftHandoffConfirmationState = value;
                OnPropertyChanged("LeftHandoffConfirmationState");
            }
        }

        /// <summary>
        ///     Gets the current state of handoff message for user on the right.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState RightHandoffMessageState
        {
            get { return rightHandoffMessageState; }

            protected set
            {
                rightHandoffMessageState = value;
                OnPropertyChanged("RightHandoffMessageState");
            }
        }

        /// <summary>
        ///     Gets the current handoff message text for user on the right.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public string RightHandoffMessageText
        {
            get { return rightHandoffMessageText; }

            protected set
            {
                rightHandoffMessageText = value;
                OnPropertyChanged("RightHandoffMessageText");
            }
        }

        /// <summary>
        ///     Gets the current handoff message background brush for user on the right.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public Brush RightHandoffMessageBrush
        {
            get { return rightHandoffMessageBrush; }

            protected set
            {
                rightHandoffMessageBrush = value;
                OnPropertyChanged("RightHandoffMessageBrush");
            }
        }

        /// <summary>
        ///     Gets the current state of handoff confirmation prompt for user on the left.
        ///     Changes to this property cause the PropertyChanged event to be signaled.
        /// </summary>
        public PromptState RightHandoffConfirmationState
        {
            get { return rightHandoffConfirmationState; }

            protected set
            {
                rightHandoffConfirmationState = value;
                OnPropertyChanged("RightHandoffConfirmationState");
            }
        }

        /// <summary>
        ///     Color used to represent the engaged user.
        /// </summary>
        public Color EngagedUserColor { get; set; }

        /// <summary>
        ///     Color used to represent non-engaged tracked users.
        /// </summary>
        public Color TrackedUserColor { get; set; }

        /// <summary>
        ///     Brush used to paint the background of a message intended for the engaged user.
        /// </summary>
        public Brush EngagedUserMessageBrush { get; set; }

        /// <summary>
        ///     Brush used to paint the background of a message intended for non-engaged tracked users.
        /// </summary>
        public Brush TrackedUserMessageBrush { get; set; }

        /// <summary>
        ///     Dictionary mapping user tracking Ids to colors corresponding to those users in
        ///     UI shown before initial engagement.
        /// </summary>
        public Dictionary<int, Color> PreEngagementUserColors { get; private set; }

        /// <summary>
        ///     Dictionary mapping user tracking Ids to colors corresponding to those users in
        ///     UI shown after initial engagement.
        /// </summary>
        public Dictionary<int, Color> PostEngagementUserColors { get; private set; }

        /// <summary>
        ///     Should be called whenever the set of actively tracked hand pointers
        ///     is updated.
        /// </summary>
        /// <param name="handPointers">
        ///     Collection of hand pointers currently being tracked.
        /// </param>
        internal void OnHandPointersUpdated(ICollection<HandPointer> handPointers)
        {
            engagementStateManager.UpdateHandPointers(handPointers);
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs e)
        {
            KinectSensor oldSensor = e.OldSensor;
            KinectSensor newSensor = e.NewSensor;

            if (null != oldSensor)
            {
                try
                {
                    oldSensor.SkeletonFrameReady -= OnSkeletonFrameReady;
                    oldSensor.SkeletonStream.AppChoosesSkeletons = false;
                    oldSensor.DepthStream.Range = DepthRange.Default;
                    oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    oldSensor.DepthStream.Disable();
                    oldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }

                engagementStateManager.Reset();
            }

            if (null != newSensor)
            {
                try
                {
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    newSensor.SkeletonStream.Enable();

                    try
                    {
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.AppChoosesSkeletons = true;
                    newSensor.SkeletonFrameReady += OnSkeletonFrameReady;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            // Whenever the Kinect sensor changes, we have no controlling user, so reset to attract screen
            NavigationManager.NavigateToHome(DefaultNavigableContexts.AttractScreen);
            IsUserEngaged = false;
        }

        private void Cleanup()
        {
            sensorChooser.Stop();
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool haveSkeletons = false;
            long timestamp = 0;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    if ((null == skeletons) || (skeletons.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    // Let engagement state manager choose which users to track.
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    timestamp = skeletonFrame.Timestamp;
                    haveSkeletons = true;
                }
            }

            if (haveSkeletons)
            {
                engagementStateManager.ChooseTrackedUsers(skeletons, timestamp, recommendedUserTrackingIds);

                var sensor = sender as KinectSensor;
                if (null != sensor)
                {
                    try
                    {
                        sensor.SkeletonStream.ChooseSkeletons(recommendedUserTrackingIds[0],
                            recommendedUserTrackingIds[1]);
                    }
                    catch (InvalidOperationException)
                    {
                        // KinectSensor might enter an invalid state while choosing skeletons.
                        // E.g.: sensor might be abruptly unplugged.
                    }
                }
            }
        }

        /// <summary>
        ///     Handler for KinectRegion.QueryPrimaryUserTrackingIdCallback.
        /// </summary>
        /// <param name="proposedTrackingId">
        ///     Tracking Id of proposed primary user.
        /// </param>
        /// <param name="candidateHandPointers">
        ///     Collection of information about hand pointers from which client can choose\
        ///     a primary user.
        /// </param>
        /// <param name="timestamp">
        ///     Time when delegate was called. Corresponds to InteractionStream and
        ///     KinectSensor event timestamps.
        /// </param>
        /// <returns>
        ///     The tracking Id of chosen primary user. 0 means that no user should be considered primary.
        /// </returns>
        private int OnQueryPrimaryUserCallback(int proposedTrackingId, IEnumerable<HandPointer> candidateHandPointers,
            long timestamp)
        {
            return engagementStateManager.QueryPrimaryUserCallback(proposedTrackingId, candidateHandPointers);
        }

        /// <summary>
        ///     Handler for engagement confirmation command.
        /// </summary>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        /// <remarks>
        ///     If this event is triggered, it means that a user has confirmed the
        ///     intent to engage while no other user was engaged.
        /// </remarks>
        private void OnEngagementConfirmation(RoutedEventArgs e)
        {
            if (engagementStateManager.ConfirmCandidateEngagement(engagementStateManager.CandidateUserTrackingId))
            {
                UpdateEngagementConfirmationState();
            }
        }

        /// <summary>
        ///     Handler for engagement handoff confirmation command.
        /// </summary>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        /// <remarks>
        ///     If this event is triggered, it means that a user has confirmed the
        ///     intent to engage when another user was already engaged.
        /// </remarks>
        private void OnEngagementHandoffConfirmation(RoutedEventArgs e)
        {
            if (engagementStateManager.ConfirmCandidateEngagement(engagementStateManager.CandidateUserTrackingId))
            {
                UpdateEngagementHandoffBarrier();
                UpdateEngagementHandoffState(true);
            }
        }

        /// <summary>
        ///     Event handler for EngagementStateManager.TrackedUsersChanged.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnEngagementManagerTrackedUsersChanged(object sender, EventArgs e)
        {
            UpdateUserTracked();
            UpdateUserColors();
            UpdateEngagementHandoffState(false);
        }

        /// <summary>
        ///     Event handler for EngagementStateManager.EngagedUserChanged.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnEngagementManagerEngagedUserChanged(object sender, UserTrackingIdChangedEventArgs e)
        {
            UpdateUserEngaged();
        }

        /// <summary>
        ///     Event handler for EngagementStateManager.CandidateUserChanged.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnEngagementManagerCandidateUserChanged(object sender, UserTrackingIdChangedEventArgs e)
        {
            IsUserEngagementCandidate = EngagementStateManager.InvalidUserTrackingId != e.NewValue;
        }

        /// <summary>
        ///     Event handler for EngagementStateManager.PrimaryUserChanged.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnEngagementManagerPrimaryUserChanged(object sender, UserTrackingIdChangedEventArgs e)
        {
            UpdateCurrentNavigationContextState();
        }

        /// <summary>
        ///     Update pre-engagement and post-engagement colors to be displayed in user viewers
        ///     based on tracked, engaged and candidate users.
        /// </summary>
        private void UpdateUserColors()
        {
            PreEngagementUserColors.Clear();
            PostEngagementUserColors.Clear();

            foreach (int trackingId in engagementStateManager.TrackedUserTrackingIds)
            {
                if (trackingId == engagementStateManager.EngagedUserTrackingId)
                {
                    PreEngagementUserColors[trackingId] = EngagedUserColor;
                    PostEngagementUserColors[trackingId] = EngagedUserColor;
                }
                else
                {
                    PreEngagementUserColors[trackingId] = EngagedUserColor;

                    if ((engagementStateManager.EngagedUserTrackingId == EngagementStateManager.InvalidUserTrackingId) ||
                        (engagementStateManager.EngagedUserTrackingId != engagementStateManager.PrimaryUserTrackingId))
                    {
                        // Differentiate tracked users from background users only if there is no
                        // engaged user currently interacting.
                        PostEngagementUserColors[trackingId] = TrackedUserColor;
                    }
                }
            }
        }

        /// <summary>
        ///     Update value of IsUserEngaged property from other properties that affect it.
        /// </summary>
        private void UpdateUserEngaged()
        {
            IsUserEngaged = IsInEngagementOverrideMode
                            ||
                            (EngagementStateManager.InvalidUserTrackingId !=
                             engagementStateManager.EngagedUserTrackingId);
        }

        /// <summary>
        ///     Update value of IsUserActive property from other properties that affect it.
        /// </summary>
        private void UpdateUserActive()
        {
            IsUserActive = IsUserEngagementCandidate || IsUserEngaged;
        }

        /// <summary>
        ///     Update value of IsUserTracked property from other properties that affect it.
        /// </summary>
        private void UpdateUserTracked()
        {
            IsUserTracked = IsInEngagementOverrideMode || (engagementStateManager.TrackedUserTrackingIds.Count > 0);
        }

        /// <summary>
        ///     Update value of StartBannerState property from other properties that affect it.
        /// </summary>
        private void UpdateStartBannerState()
        {
            var state = PromptState.Hidden;

            if (IsUserTracked)
            {
                state = IsUserEngagementCandidate || IsUserEngaged ? PromptState.Dismissed : PromptState.Prompting;
            }

            StartBannerState = state;
        }

        /// <summary>
        ///     Update value of EngagementConfirmationState property from other properties that affect it.
        /// </summary>
        private void UpdateEngagementConfirmationState()
        {
            var state = PromptState.Hidden;

            if (IsUserEngaged)
            {
                state = PromptState.Dismissed;
            }
            else if (IsUserEngagementCandidate)
            {
                state = PromptState.Prompting;
            }

            EngagementConfirmationState = state;
        }

        /// <summary>
        ///     Update value of IsEngagementHandoffBarrierEnabled property from other properties that affect it.
        /// </summary>
        private void UpdateEngagementHandoffBarrier()
        {
            IsEngagementHandoffBarrierEnabled = IsUserEngaged && IsUserEngagementCandidate;
        }

        /// <summary>
        ///     Update values of properties related to engagement handoff from the values of other properties that
        ///     affect them.
        /// </summary>
        private void UpdateEngagementHandoffState(bool confirmHandoff)
        {
            if (handoffConfirmationStasisTimer.IsEnabled)
            {
                // If timer is already running, wait for it to finish
                return;
            }

            if (confirmHandoff)
            {
                // If confirming handoff, mark handoff confirmation prompts as
                // dismissed and start timer to re-update state later.
                ClearEngagementHandoff();
                LeftHandoffConfirmationState = PromptState.Dismissed;
                RightHandoffConfirmationState = PromptState.Dismissed;
                handoffConfirmationStasisTimer.Start();

                return;
            }

            if ((engagementStateManager.EngagedUserTrackingId == EngagementStateManager.InvalidUserTrackingId) ||
                (engagementStateManager.EngagedUserTrackingId == engagementStateManager.PrimaryUserTrackingId) ||
                (engagementStateManager.TrackedUserTrackingIds.Count < 2))
            {
                // If we're currently transitioning engagement states, if there is no engaged
                // user, if engaged user is actively interacting, or there is nobody besides the
                // engaged user, then there is no need for engagement handoff UI to be shown.
                ClearEngagementHandoff();
                return;
            }

            int nonEngagedId =
                engagementStateManager.TrackedUserTrackingIds.FirstOrDefault(
                    trackingId => trackingId != engagementStateManager.EngagedUserTrackingId);

            SkeletonPoint? lastEngagedPosition =
                engagementStateManager.TryGetLastPositionForId(engagementStateManager.EngagedUserTrackingId);
            SkeletonPoint? lastNonEngagedPosition = engagementStateManager.TryGetLastPositionForId(nonEngagedId);

            if (!lastEngagedPosition.HasValue || !lastNonEngagedPosition.HasValue)
            {
                // If we can't determine the relative position of engaged and non-engaged user,
                // we don't show an engagement handoff prompt at all.
                ClearEngagementHandoff();
                return;
            }

            var engagedMessageState = PromptState.Hidden;
            string engagedMessageText = string.Empty;
            Brush engagedBrush = EngagedUserMessageBrush;
            const PromptState engagedConfirmationState = PromptState.Hidden;
            const PromptState nonEngagedMessageState = PromptState.Prompting;
            string nonEngagedMessageText = Resources.EngagementHandoffGetStarted;
            Brush nonEngagedBrush = TrackedUserMessageBrush;
            var nonEngagedConfirmationState = PromptState.Hidden;

            if ((EngagementStateManager.InvalidUserTrackingId != engagementStateManager.CandidateUserTrackingId) &&
                (nonEngagedId == engagementStateManager.CandidateUserTrackingId))
            {
                // If non-engaged user is an engagement candidate
                engagedMessageState = PromptState.Prompting;
                engagedMessageText = Resources.EngagementHandoffKeepControl;
                nonEngagedMessageText = string.Empty;
                nonEngagedConfirmationState = PromptState.Prompting;
            }

            bool isEngagedOnLeft = lastEngagedPosition.Value.X < lastNonEngagedPosition.Value.X;

            LeftHandoffMessageState = isEngagedOnLeft ? engagedMessageState : nonEngagedMessageState;
            LeftHandoffMessageText = isEngagedOnLeft ? engagedMessageText : nonEngagedMessageText;
            LeftHandoffMessageBrush = isEngagedOnLeft ? engagedBrush : nonEngagedBrush;
            LeftHandoffConfirmationState = isEngagedOnLeft ? engagedConfirmationState : nonEngagedConfirmationState;
            RightHandoffMessageState = isEngagedOnLeft ? nonEngagedMessageState : engagedMessageState;
            RightHandoffMessageText = isEngagedOnLeft ? nonEngagedMessageText : engagedMessageText;
            RightHandoffMessageBrush = isEngagedOnLeft ? nonEngagedBrush : engagedBrush;
            RightHandoffConfirmationState = isEngagedOnLeft ? nonEngagedConfirmationState : engagedConfirmationState;
        }

        /// <summary>
        ///     Reset properties related to engagement handoff to their default values.
        /// </summary>
        private void ClearEngagementHandoff()
        {
            LeftHandoffMessageState = PromptState.Hidden;
            LeftHandoffMessageText = string.Empty;
            LeftHandoffConfirmationState = PromptState.Hidden;
            RightHandoffMessageState = PromptState.Hidden;
            RightHandoffMessageText = string.Empty;
            RightHandoffConfirmationState = PromptState.Hidden;
        }

        /// <summary>
        ///     Update state of current navigation context based on current controller state.
        /// </summary>
        private void UpdateCurrentNavigationContextState()
        {
            var viewModel = NavigationManager.CurrentNavigationContext as ViewModelBase;
            if (null != viewModel)
            {
                int primaryUserTrackingId = engagementStateManager.PrimaryUserTrackingId;
                int engagedUserTrackingId = engagementStateManager.EngagedUserTrackingId;

                // Application views should only care about interaction state of currently engaged user
                viewModel.IsUserInteracting = IsInEngagementOverrideMode
                                              ||
                                              ((primaryUserTrackingId != EngagementStateManager.InvalidUserTrackingId)
                                               && (primaryUserTrackingId == engagedUserTrackingId));
            }
        }

        /// <summary>
        ///     Start timer used to navigate back to attract screen after user disengagement, with a
        ///     timeout dependent on whether there are still other users present that might want to
        ///     engage and prevent navigation.
        /// </summary>
        private void StartDisengagementNavigationTimer()
        {
            bool isAnotherUserTracked =
                engagementStateManager.TrackedUserTrackingIds.Any(
                    trackingId => trackingId != EngagementStateManager.InvalidUserTrackingId);

            disengagementNavigationTimer.Interval = isAnotherUserTracked
                ? disengagementHandoffNavigationTimeout
                : disengagementNoHandoffNavigationTimeout;
            disengagementNavigationTimer.Start();
        }

        /// <summary>
        ///     Navigate to the appropriate view given a recent change in engagement state.
        /// </summary>
        private void PerformEngagementChangeNavigation()
        {
            if (disengagementNavigationTimer.IsEnabled)
            {
                disengagementNavigationTimer.Stop();

                if (!IsUserEngaged)
                {
                    // If disengagement timer was already started, and another user got disengaged, reset timer
                    StartDisengagementNavigationTimer();
                }
                //// Else if a user just became engaged while waiting for disengagement timer to fire, don't take
                //// any navigation actions
            }
            else if (!disengagementNavigationTimer.IsEnabled)
            {
                if (IsUserEngaged)
                {
                    // If there was no engaged user and now there is, initiate a navigation to the home screen.
                    NavigationManager.NavigateToHome(DefaultNavigableContexts.HomeScreen);
                }
                else
                {
                    // Wait until timeout period before navigating to attract scren
                    StartDisengagementNavigationTimer();
                }
            }
            //// Else If we have just changed between interacting users, no navigation action is undertaken
        }

        /// <summary>
        ///     Event handler for the Tick event of the handoff confirmation stasis timer.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        /// <remarks>
        ///     If this timer fires, it means that stasis period has expired, so it is time
        ///     to confirm engagement handoff state.
        /// </remarks>
        private void OnHandoffConfirmationStasisTimerTick(object sender, EventArgs e)
        {
            handoffConfirmationStasisTimer.Stop();
            UpdateEngagementHandoffState(false);
        }

        /// <summary>
        ///     Event handler for the Tick event of the disengagement navigation timer.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        /// <remarks>
        ///     If this timer fires it means that nobody took control of application after
        ///     the previously engaged user became disengaged.
        /// </remarks>
        private void OnDisengagementNavigationTick(object sender, EventArgs e)
        {
            // If a user disengaged and nobody took control before timer expired, go back to attract screen
            disengagementNavigationTimer.Stop();
            NavigationManager.NavigateToHome(DefaultNavigableContexts.AttractScreen);
        }

        /// <summary>
        ///     Event handler for NavigationManager.PropertyChanged.
        /// </summary>
        /// <param name="sender">
        ///     Object that sent the event.
        /// </param>
        /// <param name="e">
        ///     Event arguments.
        /// </param>
        private void OnNavigationManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("CurrentNavigationContext".Equals(e.PropertyName))
            {
                UpdateCurrentNavigationContextState();
            }
        }
    }
}