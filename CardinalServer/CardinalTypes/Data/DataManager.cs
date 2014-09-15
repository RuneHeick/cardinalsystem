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
        BiDictionary<long, WeakReference> ItemCache = new BiDictionary<long, WeakReference>();
        private readonly object CacheLock = new object();

        private FileNumberNamer Namer;
        FileManager fileHandler; 

        public DataManager(string Path)
        {
            Namer = new FileNumberNamer(Path);
            fileHandler = new FileManager(Path);
        }

        public ManagedData this[int index]
        {
            get
            {
                lock (CacheLock)
                {
                    if (ItemCache.Contains(index))
                    {
                        return ItemCache[index].Target as ManagedData;
                    }
                }

                return fileHandler.LoadFile(index);
            }
            set
            {
                throw new InvalidOperationException(); 
            }
        }

        public long Add(ManagedData item)
        {
            long index = Namer.GetNext(); 
            lock (CacheLock)
                ItemCache.Add(index, new WeakReference(item));
            item.DisposeFunction = (o) => garbageCollection(index,o);
            return index; 
        }
        public void Remove(long item)
        {
            lock (CacheLock)
            {
                if (ItemCache.Contains(item))
                {
                    WeakReference data = ItemCache[item];
                    if (data != null && data.Target != null)
                    {
                        (data.Target as ManagedData).DisposeFunction = null;
                    }
                    ItemCache.Remove(item);
                }
                fileHandler.Delete(item);
                Namer.FreeNumber(item); 
            }
        }

        private void garbageCollection(long index, ManagedData data)
        {
            lock (CacheLock)
            {
                ItemCache.Remove(index);
                fileHandler.SaveItem(index, data);
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

    }

}
