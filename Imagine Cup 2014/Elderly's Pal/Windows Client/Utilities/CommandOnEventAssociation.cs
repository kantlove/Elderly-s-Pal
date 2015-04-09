using System;
using System.Windows;
using System.Windows.Input;

namespace NiceDreamers.Windows.Utilities
{
    public class CommandOnEventAssociation : Freezable
    {
        /// <summary>
        ///     Dependency property storing the current command to execute when the associated event is signaled.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof (ICommand),
            typeof (CommandOnEventAssociation),
            new PropertyMetadata(null));

        /// <summary>
        ///     Dependency property storing the name of the event to hook.
        /// </summary>
        public static readonly DependencyProperty EventProperty = DependencyProperty.Register(
            "Event",
            typeof (string),
            typeof (CommandOnEventAssociation),
            new PropertyMetadata(string.Empty));

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }

            set { SetValue(CommandProperty, value); }
        }

        public string Event
        {
            get { return (string) GetValue(EventProperty); }

            set { SetValue(EventProperty, value); }
        }

        internal Delegate Delegate { get; set; }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }
}