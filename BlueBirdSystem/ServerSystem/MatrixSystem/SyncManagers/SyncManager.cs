using MatrixSystem.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MatrixSystem.SyncManagers
{
    class GameObjectSyncObject: IDisposable
    {
        public GameObject gameObject { get; set; }

        public uint SyncCounter { get; set; }

        private List<byte[]> _syncPacket = new List<byte[]>();
        public List<byte[]> SyncPacket
        {
            get
            {
                if(_syncPacket.Count == 0)
                {
                    List<byte[]> item = gameObject.getBytes((uint)SyncMask);
                    _syncPacket.AddRange(item);
                    SyncMask = 0;
                }
                return _syncPacket;
            }
        }

        private List<byte[]> _fullPacket = new List<byte[]>();
        public List<byte[]> FullPacket
        {
            get
            {
                if(_fullPacket.Count == 0)
                {
                    List<byte[]> item = gameObject.getBytes(uint.MaxValue);
                    _fullPacket.AddRange(item);
                    SyncMask = 0;
                }
                return _fullPacket;
            }
        }

        private ulong SyncMask { get; set; }

        public GameObjectSyncObject(GameObject gameObject)
        {
            this.gameObject = gameObject;

            SyncCounter = 0;
            SyncMask = 0; 
            gameObject.MatrixChanged += GameObject_MatrixChanged;
        }

        public void Dispose()
        {
            gameObject.MatrixChanged -= GameObject_MatrixChanged;
        }

        private void GameObject_MatrixChanged(GameObject obj, ulong changedArray)
        {
            if (SyncMask == 0)
            {
                SyncCounter++;
                _syncPacket = new List<byte[]>();
                _fullPacket = new List<byte[]>();
            }

            SyncMask |= changedArray; 
        }

    }

    public class SyncClient
    {
        public VirtualSocket Socket { get; set; }
        
        public List<ObjectSubscription> Subscriptions { get; set; }
        public Dictionary<uint, ObjectSubscription> SubCache { get; set; }

        public void Add(uint id)
        {
            ObjectSubscription item = new ObjectSubscription(id) { IsSubscription = true };
            Subscriptions.Add(item);
            SubCache.Add(id,item);
        }


        public SyncClient(VirtualSocket socket)
        {
            this.Socket = socket;
            Subscriptions = new List<ObjectSubscription>();
            SubCache = new Dictionary<uint, ObjectSubscription>();
        }
    }


    public class SyncManager
    {
        public GridController controller = new GridController();
        private Dictionary<UInt32, GameObjectSyncObject> dictionary = new Dictionary<uint, GameObjectSyncObject>();
        private NeighbourSectorSync neighbourSectorSync;

        private uint _updateCount = 1;
        private List<SyncClient> subscriptions = new List<SyncClient>();
        
        public SyncManager()
        {
            neighbourSectorSync = new NeighbourSectorSync(dictionary);
            controller.OnItemZoneChanged += Controller_OnItemZoneChanged;
        }

        public void UpdateGameLoop()
        {
            controller.UpdateMatrix();
            UpdateSync();
        }

        public void Add(GameObject gameObject)
        {
            dictionary.Add(gameObject.ObjectID, new GameObjectSyncObject(gameObject));
            controller.AddGameObject(gameObject);
        }

        public void Remove(GameObject gameObject)
        {
            GameObjectSyncObject item = null;
            if (dictionary.TryGetValue(gameObject.ObjectID, out item))
            {
                dictionary.Remove(gameObject.ObjectID);
                item.Dispose();
            }
        }

        public void AddSubscription(SyncClient sync)
        {
            subscriptions.Add(sync);
        }

        private void Controller_OnItemZoneChanged(GameObject gameObject, ObjectMatrix.Types.PositionValue position, ZoneTypes currentZone, ZoneTypes oldZone)
        {
            if(currentZone == ZoneTypes.OUT_OF_AREA)
            {
                Remove(gameObject);
            }
            else
            {
                
            }
        }

        private void UpdateSync()
        {
            foreach (var sub in subscriptions)
            {
                List<byte[]> packet = getClientPacket(sub, _updateCount);
   
                sub.Socket.Send(packet);
            }
            _updateCount++;
        }

        private List<byte[]> getClientPacket(SyncClient item, uint SyncCount)
        {
            List<uint> close_ids = new List<uint>();
            List<byte[]> bytes = new List<byte[]>();
            List<ObjectSubscription> needRemoveSub = new List<ObjectSubscription>();
            foreach (var objectId in item.Subscriptions)
            {
                if (objectId.LastUpdateCounter != SyncCount)
                {
                    GameObjectSyncObject gameItem;
                    if (dictionary.TryGetValue(objectId.Id, out gameItem))
                    {
                        objectId.LastUpdateCounter = SyncCount;
                        controller.GetClose(gameItem.gameObject, (GameObject) => UpdateCache(item.SubCache, GameObject, SyncCount));
                        UpdatePacket(bytes, gameItem, objectId);
                    }
                    else
                    {
                        needRemoveSub.Add(objectId);
                    }
                }
            }

            needRemoveSub.ForEach((i) => item.Subscriptions.Remove(i));

            List<uint> needRemove = new List<uint>();
            foreach (var packetElement in item.SubCache.Values)
            {
                if(!packetElement.IsSubscription)
                {
                    if(packetElement.LastUpdateCounter == SyncCount)
                    {
                        GameObjectSyncObject gameItem;
                        if (dictionary.TryGetValue(packetElement.Id, out gameItem))
                        {
                            UpdatePacket(bytes, gameItem, packetElement);
                        }
                        else
                        {
                            needRemove.Add(packetElement.Id);
                        }
                    }
                    else
                    {
                        needRemove.Add(packetElement.Id);
                    }
                }                
            }

            needRemove.ForEach((i) => item.SubCache.Remove(i));

            return bytes;
        }

        private void UpdateCache(Dictionary<uint, ObjectSubscription> cache, GameObject gameItem, uint syncCount)
        {
            ObjectSubscription item; 
            if(!cache.TryGetValue(gameItem.ObjectID, out item))
            {
                item = new ObjectSubscription(gameItem.ObjectID);
                cache.Add(gameItem.ObjectID, item);
            }
            item.LastUpdateCounter = syncCount;
        }

        private void UpdatePacket(List<byte[]> packet, GameObjectSyncObject gameItem, ObjectSubscription objectId)
        {
            if (objectId.LastSync == gameItem.SyncCounter)
            {
                return;
            }
            else if (objectId.LastSync + 1 == gameItem.SyncCounter)
            {
                packet.AddRange(gameItem.SyncPacket);
            }
            else
            {
                packet.AddRange(gameItem.FullPacket);
            }
            objectId.LastSync = gameItem.SyncCounter;
        }
        
    }
}
