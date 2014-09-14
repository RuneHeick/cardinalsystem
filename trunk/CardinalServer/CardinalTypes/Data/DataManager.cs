using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    public class DataManager
    {
        HashSet<CollectionItem> ItemCache = new HashSet<CollectionItem>();
        private readonly object CacheLock = new object();

        Timer GCTimer;
        const int GC_TIME = 5000; 

        public DataManager(string Path)
        {
            GCTimer = null; 
        }

        public ValueContainor Add(IData item)
        {
            CollectionItem c = new CollectionItem();
            ValueContainor V = c.Populate(item);
            lock (CacheLock)
            {
                
                ItemCache.Add(c);
                if (GCTimer == null)
                    GCTimer = new Timer((o) => garbageCollection(), null, GC_TIME, GC_TIME);
            }
            return V; 
        }
        public void Remove(IData item)
        {
            CollectionItem collectionItem = ItemCache.FirstOrDefault((o) => o.DataValue == item); 
            if(collectionItem != null)
            {
                lock (CacheLock)
                {
                    ItemCache.Remove(collectionItem);
                }
            }

            if (ItemCache.Count == 0)
                StopTimer();
        }

        private void garbageCollection()
        {
            for (int i = 0; i < ItemCache.Count; i++)
            {
                CollectionItem item = ItemCache.ElementAt(i); 
                if(item.Value.Target == null)
                {
                    lock(CacheLock)
                    {
                        ItemCache.Remove(item);
                    }
                    i--;
                    SaveItem(item.DataValue);
                }
            }

            if (ItemCache.Count == 0)
                StopTimer();
        }

        private void StopTimer()
        {
            GCTimer.Dispose();
            GCTimer = null; 
        }

        private void SaveItem(IData item)
        {
            
        }

        class CollectionItem
        {

            public ValueContainor Populate(IData item)
            {
                ValueContainor data = new ValueContainor();
                data.Value = item;
                Value = new WeakReference(data);
                DataValue = item;
                return data;
            }

            public WeakReference Value { get; set; }
            public IData DataValue { get; set; }
        }

    }

    public class ValueContainor
    {
        public IData Value { get; set; }

    }


}
