using MatrixSystem.ObjectMatrix.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.SyncManagers
{
    class NeighbourSectorSync
    {
        private Dictionary<UInt32, GameObjectSyncObject> ManagedDictionary = new Dictionary<uint, GameObjectSyncObject>();

        private ZoneTypes[] tracked_zone = { ZoneTypes.IN_SYNC_ZONE_LOWER_LEFT, ZoneTypes.IN_SYNC_ZONE_LOWER_MID, ZoneTypes.IN_SYNC_ZONE_LOWER_RIGHT, ZoneTypes.IN_SYNC_ZONE_MID_LEFT, ZoneTypes.IN_SYNC_ZONE_MID_RIGHT, ZoneTypes.IN_SYNC_ZONE_UPPER_LEFT, ZoneTypes.IN_SYNC_ZONE_UPPER_MID, ZoneTypes.IN_SYNC_ZONE_UPPER_RIGHT };
        private uint max_zone_id = uint.MinValue;
        private uint min_zone_id = uint.MaxValue;
        private List<ObjectSubscription>[] subscriptions;

        public NeighbourSectorSync(Dictionary<UInt32, GameObjectSyncObject> dictionary)
        {
            Array.Sort(tracked_zone, (a, b) => (((uint)a) > ((uint)b)) ? 1 : -1);
            max_zone_id = (uint)tracked_zone[tracked_zone.Length - 1];
            min_zone_id = (uint)tracked_zone[0];

            subscriptions = new List<ObjectSubscription>[(max_zone_id - min_zone_id)+1];
            ManagedDictionary = dictionary;
        }

        public void GameObject_OnItemZoneChanged(GameObject gameObject, PositionValue position, ZoneTypes currentZone, ZoneTypes oldZone)
        {

        }

    }
}
