using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server2.Utility
{
    public class EventManager<T, A>
    {
        private readonly List<Event> _events = new List<Event>();

        public EventManager(EventManagerSetupInfo<T,A> info)
        {
            info.Publish = Publish; 
        }


        public void Unsubscribe(Action<T, A> eventHandler)
        {
            lock (_events)
            {
                for (var i = 0; i < _events.Count; i++)
                {
                    if (_events[i].eventHandler.Equals(eventHandler))
                    {
                        _events.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public void Subscribe(Action<T, A> eventHandler, Func<T, A, bool> condition = null)
        {
            var eventInfo = new Event(eventHandler, condition);
            lock (_events)
            {
                _events.Add(eventInfo);
            }
        }

        private void Publish(T item, A sender)
        {
            lock (_events)
            {
                for (var i = 0; i < _events.Count; i++)
                {
                    var eventInfo = _events[i];
                    if (eventInfo.condition == null || eventInfo.condition(item, sender))
                    {
                        Task.Factory.StartNew(() => eventInfo.eventHandler(item, sender));
                    }
                }
            }
        }

        private class Event
        {
            public Event(Action<T, A> eventHandler, Func<T, A, bool> condition)
            {
                this.eventHandler = eventHandler;
                this.condition = condition;
            }

            public Action<T, A> eventHandler { get; private set; }
            public Func<T, A, bool> condition { get; private set; }
        }
    }

    public class EventManagerSetupInfo<T, A>
    {
        public Action<T, A> Publish { get; set; }
    }
}