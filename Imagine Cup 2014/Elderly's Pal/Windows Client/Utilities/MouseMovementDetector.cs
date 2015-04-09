using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NiceDreamers.Windows.Utilities
{
    /// <summary>
    ///     Keeps track of mouse movement over a specified window and sends events whenever
    ///     movement state (moving versus not moving for a long enough period of time) changes.
    /// </summary>
    public class MouseMovementDetector
    {
        /// <summary>
        ///     Interval for which the mouse must be stationary before we decide it's not moving anymore.
        /// </summary>
        private const double StationaryMouseIntervalInMilliseconds = 3000;

        /// <summary>
        ///     Timer used to determine whether mouse has not moved for long enough to call it stationary.
        /// </summary>
        private readonly DispatcherTimer timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(StationaryMouseIntervalInMilliseconds)
        };

        /// <summary>
        ///     Window for which mouse movement is being monitored.
        /// </summary>
        private readonly Window window;

        /// <summary>
        ///     true if mouse has moved recently, false if mouse is stationary.
        /// </summary>
        private bool isMoving;

        /// <summary>
        ///     Last mouse position (expressed in screen coordinates) observed.
        /// </summary>
        private Point? lastMousePosition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MouseMovementDetector" /> class.
        /// </summary>
        /// <param name="window">
        ///     Window for which mouse movement will be monitored.
        /// </param>
        public MouseMovementDetector(Window window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            this.window = window;

            timer.Tick += (s, args) =>
            {
                // Mouse is now stationary, so we should update movement state and stop timer
                IsMoving = false;

                timer.Stop();
            };
        }

        /// <summary>
        ///     true if mouse has moved recently, false if mouse is stationary.
        /// </summary>
        public bool IsMoving
        {
            get { return isMoving; }

            set
            {
                bool oldValue = isMoving;

                isMoving = value;

                if ((oldValue != value) && (IsMovingChanged != null))
                {
                    IsMovingChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     Event triggered whenever IsMoving property value changes.
        /// </summary>
        public event EventHandler<EventArgs> IsMovingChanged;

        /// <summary>
        ///     Starts tracking mouse movement.
        /// </summary>
        public void Start()
        {
            window.MouseMove += OnMouseMove;
        }

        /// <summary>
        ///     Stops tracking mouse movement.
        /// </summary>
        public void Stop()
        {
            window.MouseMove -= OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            // Use mouse position in the screen relative coordinate system as hiding/showing the bezel changes the client-area position
            Point mousePosition = window.PointToScreen(e.GetPosition(window));

            if (lastMousePosition.HasValue && lastMousePosition.Value != mousePosition)
            {
                IsMoving = true;
                timer.Stop();
                timer.Start();
            }

            lastMousePosition = mousePosition;
        }
    }
}