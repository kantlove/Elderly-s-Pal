using System;
using System.Collections.Generic;

namespace NiceDreamers.Windows.Utilities
{
    /// <summary>
    ///     Utility class that encapsulates queuing events triggered while within the
    ///     section to be sent all together when we exit the lock.  Its purpose in life
    ///     is to delay calling event handlers until all related state has been changed,
    ///     so that when a client receives an event related to a specific state
    ///     property, the data for all related properties is guaranteed to be
    ///     consistent.
    /// </summary>
    internal class EventQueueSection : IDisposable
    {
        public delegate void ExitEventHandler();

        private readonly Queue<ExitEventHandler> eventHandlerQueue = new Queue<ExitEventHandler>();

        internal int ItemCount
        {
            get { return eventHandlerQueue.Count; }
        }

        public void Dispose()
        {
            while (eventHandlerQueue.Count > 0)
            {
                ExitEventHandler handler = eventHandlerQueue.Dequeue();
                handler();
            }
        }

        public void Enqueue(ExitEventHandler handler)
        {
            eventHandlerQueue.Enqueue(handler);
        }
    }
}