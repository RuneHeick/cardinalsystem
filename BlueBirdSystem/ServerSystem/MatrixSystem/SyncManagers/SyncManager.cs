using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Networking.Util;
using MatrixSystem.ObjectCreation;
using MatrixSystem.Network;

namespace MatrixSystem.SyncManagers
{
    class GameObjectSyncObject: IDisposable
    {
        public static uint PublicSyncMask; 
    
        public GameObject gameObject { get; set; }

        public uint SyncCounter { get; set; }

        static GameObjectSyncObject()
        {
            PublicSyncMask = 0;
            for (int i = 0; i < (uint)PublicSyncTypes.MAX_VALUE; i++)
                PublicSyncMask |= (uint)(1 << i);
        }

        private List<byte[]> _syncPacket = new List<byte[]>();
        public List<byte[]> SyncPacket
        {
            get
            {
                if(_syncPacket.Count == 0)
                {
                    List<byte[]> header = NetworkUpdateHandler.GetSyncHeader(gameObject);
                    List<byte[]> item = gameObject.getBytes((uint)SyncMask);
                    _syncPacket.AddRange(header);
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
                    List<byte[]> header = NetworkUpdateHandler.GetFullHeader(gameObject);
                    List<byte[]> item = gameObject.getBytes(uint.MaxValue);
                    _fullPacket.AddRange(header);
                    _fullPacket.AddRange(item);
                    SyncMask = 0;
                }
                return _fullPacket;
            }
        }

        private List<byte[]> _fullPublicPacket = new List<byte[]>();
        public List<byte[]> FullPublicPacket
        {
            get
            {
                if (_fullPublicPacket.Count == 0)
                {
                    List<byte[]> header = NetworkUpdateHandler.GetFullHeader(gameObject);
                    List<byte[]> item = gameObject.getBytes(PublicSyncMask);
                    _fullPublicPacket.AddRange(header);
                    _fullPublicPacket.AddRange(item);
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
            gameObject.GameObjectDataChanged += GameObject_MatrixChanged;
        }

        public void Dispose()
        {
            gameObject.GameObjectDataChanged -= GameObject_MatrixChanged;
        }

        private void GameObject_MatrixChanged(GameObject obj, ulong changedArray)
        {
            if (SyncMask == 0)
            {
                SyncCounter++;
                _syncPacket = new List<byte[]>();
                _fullPacket = new List<byte[]>();
                _fullPublicPacket = new List<byte[]>();
            }

            SyncMask |= changedArray; 
        }
    }

    public class SyncManager
    {
        public GridController controller = new GridController();
        private Dictionary<UInt32, GameObjectSyncObject> dictionary = new Dictionary<uint, GameObjectSyncObject>();
        private NeighbourSectorSync neighbourSectorSync;

        private uint _updateCount = 1;
        private List<SyncClient> subscriptions = new List<SyncClient>();
        private List<GameObject> removeGameObject = new List<GameObject>();
        
        public SyncManager()
        {
            neighbourSectorSync = new NeighbourSectorSync(dictionary);
            controller.OnItemZoneChanged += Controller_OnItemZoneChanged;
        }

        public void UpdateGameLoop()
        {
            //Read in packets from neighbouring sectors 

            //Update gameloop for the gameobjects 
            controller.UpdateMatrix();
            //gennerate packet with sync data to the clients 
            UpdateSync();

            //Clean up after Sync
            GameLoopCleanUp();            
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

        public void RemoveSubscription(SyncClient sync)
        {
            subscriptions.Remove(sync);
        }

        public void AddBorderSubscription(SyncClient sync, ZoneTypes syncZone)
        {
            neighbourSectorSync.AddSyncItem(sync, syncZone);
        }

        public void RemoveBorderSubscription(SyncClient sync, ZoneTypes syncZone)
        {
            neighbourSectorSync.RemoveSyncItem(sync, syncZone);
        }

        private void Controller_OnItemZoneChanged(GameObject gameObject, ObjectMatrix.Types.PositionValue position, ZoneTypes currentZone, ZoneTypes oldZone)
        {
            neighbourSectorSync.GameObject_OnItemZoneChanged(gameObject, position, currentZone, oldZone);
            if(currentZone == ZoneTypes.OUT_OF_AREA)
            {
                removeGameObject.Add(gameObject);
            }
        }

        private void UpdateSync()
        {
            foreach (var sub in subscriptions.Concat(neighbourSectorSync))
            {
                ComposedMessage packet = getClientPacket(sub, _updateCount);
   
                sub.Socket.Send(packet);
            }

            _updateCount++;
        }

        private void GameLoopCleanUp()
        {
            neighbourSectorSync.ServiceAfterSync();
            foreach(var gameObject in removeGameObject)
            {
                Remove(gameObject);
            }
        }

        private ComposedMessage getClientPacket(SyncClient item, uint SyncCount)
        {
            List<uint> close_ids = new List<uint>();
            ComposedMessage bytes = new ComposedMessage();
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
                        UpdatePacket(bytes, gameItem, objectId, true);
                    }
                    else
                    {
                        needRemoveSub.Add(objectId);
                    }
                }
            }

            needRemoveSub.ForEach((i) => item.Remove(i.Id));

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
                            UpdatePacket(bytes, gameItem, packetElement, false);
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

        private void UpdatePacket(ComposedMessage packet, GameObjectSyncObject gameItem, ObjectSubscription objectId, bool privatePacket)
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
                if(privatePacket)
                    packet.AddRange(gameItem.FullPacket);
                else
                    packet.AddRange(gameItem.FullPublicPacket);
            }
            objectId.LastSync = gameItem.SyncCounter;
        }
    }
}
