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
        BiDictionary<long, CollectionItem> ItemCache = new BiDictionary<long, CollectionItem>();
        private readonly object CacheLock = new object();

        public DataManager(string Path)
        {

        }

        public ManagedData this[int index]
        {
            get
            {
                if(ItemCache.Contains(index))
                {
                   return ItemCache[index]..Target as ManagedData; 
                }
                else
                {
                    return LoadFile(index); 
                }
            }
            set
            {
                lock (CacheLock)
                {
                    if (ItemCache.ContainsKey(index))
                    {
                        var item = ItemCache[index];
                        if (item.Ref.Target != null)
                            (item.Ref.Target as ManagedData).DisposeFunction = null;
                        ItemCache.Remove(index);
                    }
                }
               Add(value,index); 
            }
        }

        public long Add(ManagedData item, long index = 0)
        {
            item.DisposeFunction = garbageCollection;
            while (ItemCache.ContainsKey(index) && index>0)
            {
                index++;
            }
            lock (CacheLock)
                ItemCache.Add(index, new CollectionItem(item));

            return index; 
        }
        public void Remove(ManagedData item)
        {
            lock (CacheLock)
            {
                long key = ItemCache.Keys.FirstOrDefault((o) =>
                        {
                            var element = ItemCache[o];
                            if (element.Ref.Target != null)
                                return element.Ref.Target == item;
                            return false;
                        });

                if (key != 0)
                {
                    Delete(item);
                    ItemCache.Remove(key);
                    item.DisposeFunction = null;
                }
            }
        }

        private void garbageCollection(ManagedData data)
        {
            for (int i = 0; i < ItemCache.Count; i++)
            {
                var item = ItemCache.ElementAt(i);

                if (item.Ref.Target == null || item.Ref.Target == data)
                {
                    lock(CacheLock)
                    {
                        ItemCache.i(item);
                    }
                    i--;
                    SaveItem(data);
                }
            }
        }

        class CollectionItem
        {
            public CollectionItem(object target)
            {
                Ref = new WeakReference(target);
            }

            public WeakReference Ref { get; set; }
        }

        public void Delete(ManagedData item)
        {

        }

        private ManagedData LoadFile(int index)
        {
            
        }

        private void SaveItem(ManagedData item)
        {
            
        }

    }

}
