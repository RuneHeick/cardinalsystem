using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public class UdpConnector: IConnector
    {
        public IPEndPoint Me { get; private set; }
        readonly UdpClient _listener;

        public UdpConnector(IPEndPoint address)
        {
            Me = address;
            _listener = new UdpClient(Me);
            StartRecive();
        }

        public void Close()
        {
            try
            {
                _listener.Close();
            }
            catch (Exception)
            {
                //ignore 
            }
            
        }

        private void AsyncUdpInfoReciveComplete(IAsyncResult ar)
        {
            ReciverInfo info = (ReciverInfo) ar.AsyncState;
            info.TimeStamp = DateTime.Now;

            try
            {
                IPEndPoint from = new IPEndPoint(0,0);
                info.PacketBuffer = _listener.EndReceive(ar, ref from);
                info.Address = from;
                StartRecive();

                if (0 != info.PacketBuffer.Length)
                {
                    PacketsRecived(info);
                }
               
            }
            catch(ObjectDisposedException e)
            { }
            catch(Exception e)
            {
                StartRecive();
            }
        }


        private void PacketsRecived(ReciverInfo info)
        {
            try
            {
                NetworkPacket nPacket = new NetworkPacket(info.PacketBuffer, this, PacketType.Udp);
                nPacket.Address = info.Address;
                nPacket.TimeStamp = info.TimeStamp;

                // For thread safty
                Action<NetworkPacket, IConnector> Event = OnPacketRecived;
                if (Event != null)
                    Event(nPacket, this);
            }
            catch (Exception)
            {

            }
        }

        private void StartRecive()
        {
            var item = new ReciverInfo();
            _listener.BeginReceive(AsyncUdpInfoReciveComplete, item);
        }


        public void Send(NetworkPacket packet)
        {

            _listener.BeginSend(packet.Packet, packet.Packet.Length, packet.Address, AsyncCallbackComplete, null);
        }

        private void AsyncCallbackComplete(IAsyncResult ar)
        {
            try
            {
                _listener.EndSend(ar);
            }
            catch
            {
                // ignored
            }
        }

        public PacketType Supported
        {
            get { return PacketType.Udp; }
        }

        public event Action<NetworkPacket, IConnector> OnPacketRecived;
        private class ReciverInfo
        {
            public IPEndPoint Address { get; set; }

            public byte[] PacketBuffer { get; set; }

            public DateTime TimeStamp { get; set; }

        }

    }

    
}
