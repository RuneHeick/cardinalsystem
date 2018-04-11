using MatrixSystem.Network;
using System.Collections.Generic;

namespace MatrixSystem.SyncManagers
{
    public class SyncClient
    {
        public VirtualSocket Socket { get; set; }
        
        public List<ObjectSubscription> Subscriptions { get; set; }
        public Dictionary<uint, ObjectSubscription> SubCache { get; set; }

        public void Add(uint id)
        {
            ObjectSubscription item = new ObjectSubscription(id) { IsSubscription = true };
            Subscriptions.Add(item);
            ObjectSubscription cacheItem;
            if (SubCache.TryGetValue(id, out cacheItem))
            {
                cacheItem.IsSubscription = true;
            }
            else
            {
                SubCache.Add(id, item);
            }
        }

        public void Remove(uint id)
        {
            ObjectSubscription item;
            if (SubCache.Remove(id, out item))
            {
                Subscriptions.Remove(item);
            }
        }

        public SyncClient(VirtualSocket socket)
        {
            this.Socket = socket;
            Subscriptions = new List<ObjectSubscription>();
            SubCache = new Dictionary<uint, ObjectSubscription>();
        }
    }
}
