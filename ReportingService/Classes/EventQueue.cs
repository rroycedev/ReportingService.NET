using System;
using System.Threading;
using System.Collections.Generic;

using ReportingServiceDatabase.DataSets;

namespace ReportingService.Classes
{
    public class EventQueue
    {
        private Queue<ReportEvent> _events = new Queue<ReportEvent>();
        private static Mutex _queueMutex = new Mutex(false, "queuemutext");

        public EventQueue()
        {
        }

        public void AddEvent(ReportEvent e)
        {
            _queueMutex.WaitOne();

            this._events.Enqueue(e);

            _queueMutex.ReleaseMutex();
        }

        public ReportEvent GetEvent()
        {
            _queueMutex.WaitOne();

            ReportEvent e = null;

            if (this._events.Count > 0)
            {
                e = this._events.Dequeue();
            }

            _queueMutex.ReleaseMutex();

            return e;
        }

    }
}
