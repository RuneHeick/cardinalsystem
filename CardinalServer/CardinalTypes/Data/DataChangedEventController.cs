using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    public sealed class DataChangedEventController
    {
        private static volatile DataChangedEventController instance;
        private static object syncRoot = new Object();
        private TaskScheduler UpdateScheduler = new LimitedConcurrencyLevelTaskScheduler(10); 


        private Dictionary<long, List<CollectionObject>> EventHandlers = new Dictionary<long, List<CollectionObject>>(); 

        private DataChangedEventController() 
        { 

        }

        public void RegisterEventHandler(string PropertyName, ManagedData Target, ManagedData caller)
        {
            if(!EventHandlers.ContainsKey(Target.ID))
            {
                EventHandlers.Add(Target.ID, new List<CollectionObject>(1)); 
            }

            List<CollectionObject> list = EventHandlers[Target.ID];
            lock (list)
            {
                CollectionObject toAdd = new CollectionObject(caller.ID, PropertyName);
                if (!list.Contains(toAdd, new CollectionObjectComparer()))
                {
                    list.Add(toAdd);
                }
            }
        }

        public void UnregisterEventHandler(string PropertyName, ManagedData Target, ManagedData caller)
        {

            if (EventHandlers.ContainsKey(Target.ID))
            {
                List<CollectionObject> list = EventHandlers[Target.ID];
                lock (list)
                {
                    CollectionObject toRemove = list.FirstOrDefault((o) => o.CallerID == caller.ID && o.PropertyName == PropertyName);
                    if (toRemove != null)
                    {
                        list.Remove(toRemove);
                    }
                }
            }
        }

        public void OnPropertyChanged(string PropertyName, ManagedData item)
        {
            if(EventHandlers.ContainsKey(item.ID))
            {
                var list = EventHandlers[item.ID]; 
                lock(list)
                {
                    foreach(CollectionObject obj in list)
                    {
                        if(obj.PropertyName == PropertyName)
                        {
                            (new Task(() => InvokeChange(obj.CallerID, PropertyName, item))).Start(UpdateScheduler); 
                        }
                    }
                }
            }
        }

        private void InvokeChange(long Id, string PropertyName, ManagedData item)
        {
            throw new NotImplementedException(); 
            ManagedData toBeInvoked = null;  // get the caller from the main recourece Manager. 

            toBeInvoked.OnDependencyPropertyChanged(item, PropertyName); 
        }


        public static DataChangedEventController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DataChangedEventController();
                    }
                }

                return instance;
            }
        }

        private class CollectionObject
        {
            public CollectionObject(long CallerID_,string PropertyName_)
            {
                this.CallerID = CallerID_;
                this.PropertyName = PropertyName_; 
            }

            public long CallerID { get; set; }
            public string PropertyName { get; set; }
        }

        private class CollectionObjectComparer : IEqualityComparer<CollectionObject>
        {

            public bool Equals(CollectionObject x, CollectionObject y)
            {
                return x.CallerID == y.CallerID && x.PropertyName == y.PropertyName; 
            }

            public int GetHashCode(CollectionObject obj)
            {
                return obj.GetHashCode(); 
            }
        }


    }
}
