using MatrixSystem.ObjectMatrix.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.SyncManagers
{
    class NeighbourSectorSync : IEnumerable<SyncClient>
    {
        private Dictionary<UInt32, GameObjectSyncObject> ManagedDictionary = new Dictionary<uint, GameObjectSyncObject>();

        private ZoneTypes[] tracked_zone = { ZoneTypes.IN_SYNC_ZONE_LOWER_LEFT, ZoneTypes.IN_SYNC_ZONE_LOWER_MID, ZoneTypes.IN_SYNC_ZONE_LOWER_RIGHT, ZoneTypes.IN_SYNC_ZONE_MID_LEFT, ZoneTypes.IN_SYNC_ZONE_MID_RIGHT, ZoneTypes.IN_SYNC_ZONE_UPPER_LEFT, ZoneTypes.IN_SYNC_ZONE_UPPER_MID, ZoneTypes.IN_SYNC_ZONE_UPPER_RIGHT };
        private uint max_zone_id = uint.MinValue;
        private uint min_zone_id = uint.MaxValue;
        private List<SyncClient>[] subscriptions;
        private List<RemoveItem> itemsToRemove = new List<RemoveItem>();

        public NeighbourSectorSync(Dictionary<UInt32, GameObjectSyncObject> dictionary)
        {
            Array.Sort(tracked_zone, (a, b) => (((uint)a) > ((uint)b)) ? 1 : -1);
            max_zone_id = (uint)tracked_zone[tracked_zone.Length - 1];
            min_zone_id = (uint)tracked_zone[0];

            subscriptions = new List<SyncClient>[(max_zone_id - min_zone_id)+1];
            for (int i = 0; i < subscriptions.Length; i++)
                subscriptions[i] = new List<SyncClient>();
            ManagedDictionary = dictionary;
        }
        
        //Called during game loop update.
        public void GameObject_OnItemZoneChanged(GameObject gameObject, PositionValue position, ZoneTypes currentZone, ZoneTypes oldZone)
        {
            uint removeZoneIndex = ((uint)oldZone) - min_zone_id;
            uint addZoneIndex = ((uint)currentZone) - min_zone_id;

            if(removeZoneIndex >= 0 && removeZoneIndex < subscriptions.Length)
            {
                foreach(var sub in subscriptions[removeZoneIndex])
                {
                    //Schedule to remove, but keep to send one last sync message to the other servers. 
                    itemsToRemove.Add(new RemoveItem(sub, gameObject.ObjectID));
                }
            }
            
            if(addZoneIndex >= 0 && addZoneIndex < subscriptions.Length)
            {
                foreach (var sub in subscriptions[addZoneIndex])
                {
                    sub.Add(gameObject.ObjectID);
                }
            }
        }

        //Remove items, this is called after the sync loop gennerating the packets.
        public void ServiceAfterSync()
        {
            foreach(var rem in itemsToRemove)
            {
                rem.Client.Remove(rem.Id);
            }
            itemsToRemove.Clear();
        }

        public void AddSyncItem(SyncClient client, ZoneTypes zone)
        {
            uint id = ((uint)zone) - min_zone_id;
            subscriptions[id].Add(client);
        }

        public void RemoveSyncItem(SyncClient client, ZoneTypes zone)
        {
            uint id = ((uint)zone) - min_zone_id;
            subscriptions[id].Remove(client);
        }

        public IEnumerator<SyncClient> GetEnumerator()
        {
            for(int i = 0; i<subscriptions.Length; i++)
            {
                var subList = subscriptions[i]; 
                foreach(var sub in subList)
                {
                    yield return sub;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class RemoveItem
        {
            public SyncClient Client { get; set; }

            public uint Id { get; set; }

            public RemoveItem(SyncClient client, uint id)
            {
                Client = client;
                Id = id;
            }
        }

    }

    
    

}
