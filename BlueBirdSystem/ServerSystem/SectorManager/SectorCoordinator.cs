using Networking;
using Networking.Util;
using NetworkingLayer.Util;
using Protocol;
using Sector;
using SectorController.Exceptions;
using SectorController.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SectorController
{

    public delegate string GetRemoteAddressHandler(ulong sector);

    //Must return true if the packet could be send. It does not ensure the packet is received.
    //Try connect if it is a server, and not a client that the data should be send to. 
    public delegate bool SendNetworkHandler(ComposedMessage msg, string address, bool TryConnect);

    public class SectorCoordinator
    {
        private SectorManager _manager = new SectorManager();

        public event GetRemoteAddressHandler OnRequestSectorAddress;
        public event SendNetworkHandler OnSendPacketOnNetwork;

        public SectorCoordinator()
        {
            _manager.OnMsgToOther += Handler_OnMsgToOther;
            _manager.OnStatisticChanged += Sectors_StatisticChanged;
            SpawnSectors();
        }

        /* -------------------------------------- */

        public void SpawnSectors()
        {
            Task[] tasks = new Task[50];
            for(int i = 0; i< tasks.Length; i++)
            {
                int z = i;
                tasks[i] = (Task.Run(()=>_manager.Add(new TestSector((ulong)z))));
            }

            Task.WaitAll(tasks);
        }

        public void Tick()
        {
            _manager.DoTick();
        }

        /* -------------------------------------- */

        private void Sectors_StatisticChanged(SectorsStatistics statistics)
        {
            
        }


        /* -------------------------------------- */

        private void Handler_OnMsgToOther(ISector sector, IMsg msg)
        {
            IClientMsg clientMsg = msg as IClientMsg;
            ISectorMsg sectorMsg = null;

            string address = "";
            var cmsg = msg.GetMessage();

            if (clientMsg != null)
            {
                address = clientMsg.Client;
                cmsg.AddToFront(BitConverter.GetBytes(msg.ToSector));
                cmsg.AddToFront(new byte[] {(byte)NetworkMessageType.SECTOR_PLAYER_MESSAGE });
            }
            else
            {
                sectorMsg = msg as ISectorMsg;
                if(sectorMsg != null)
                {
                    address = OnRequestSectorAddress.Invoke(msg.ToSector);
                    cmsg.AddToFront(BitConverter.GetBytes(sectorMsg.FromSector));
                    cmsg.AddToFront(BitConverter.GetBytes(msg.ToSector));
                    cmsg.AddToFront(new byte[] { (byte)NetworkMessageType.SECTOR_SECTOR_MESSAGE });
                }
            }

            bool? send = OnSendPacketOnNetwork?.Invoke(cmsg, address, sectorMsg != null);
            if((send == null || send == false) && clientMsg != null)
            {
                sector.ClientOffline(clientMsg.Client);
            }
        }

        //Add "Sector is moved" error.
        //Need to check if sector does live on this server. 
        public void Network_OnSocketEvent(INetwork sender, SocketEvent arr)
        {
            if(arr.State == NetworkingLayer.ConnectionState.Data)
            {
                switch(arr.Data[0])
                {
                    case (byte)NetworkMessageType.SECTOR_PLAYER_MESSAGE:
                    case (byte)NetworkMessageType.SECTOR_SECTOR_MESSAGE:
                    {
                            IMsg packet;
                            if(arr.Data[0] == (byte)NetworkMessageType.SECTOR_SECTOR_MESSAGE)
                                packet = SectorSectorPacket.Create(arr.Data, 0);
                            else
                                packet = SectorClientPacket.Create(arr.Data, 0, arr);

                            try
                            {
                                Task.Run(() => _manager.MessageFromOtherSector(packet));
                            }
                            catch(SectorNotFoundException e)
                            {
                                SendError(Errors.SECTOR_NOT_ON_SERVER, sender, arr);
                            }
                            catch(Exception e)
                            {
                                if(e.InnerException != null && e.InnerException.GetType().Equals(typeof(SectorNotFoundException)))
                                    SendError(Errors.SECTOR_NOT_ON_SERVER, sender, arr);
                            }
                    }
                    break;
                }

            }
        }


        private void SendError(Errors error, INetwork sender, SocketEvent arr)
        {
            
        }
    }
}
