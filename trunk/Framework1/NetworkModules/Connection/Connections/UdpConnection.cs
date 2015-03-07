using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using Server3.Intercom.Network.Packets;

namespace NetworkModules.Connection.Connections
{
    public class UdpConnection : IConnection
    {
        private UdpConnector uDPConnector;
        public IPEndPoint RemoteEndPoint { get; private set; }
        public UdpConnection(UdpConnector udpConnector, System.Net.IPEndPoint endPoint)
        {
            this.uDPConnector = udpConnector;
            this.RemoteEndPoint = endPoint;
        }

        internal void AddPacket(byte[] packetBytes)
        {
            NetworkPacket packet = new NetworkPacket(packetBytes, this, PacketType.Udp)
            {
                TimeStamp = DateTime.Now,
                Address = RemoteEndPoint
            };
            OnOnPacketRecived(packet);
        }

        public PacketType Supported
        {
            get { return PacketType.Udp;}
        }

        public void Send(NetworkPacket packet)
        {
            uDPConnector.Send(packet.Packet, packet.Address); 
        }

        public void Close()
        {
            uDPConnector.Close(this); 
        }

        public event PacketHandler OnPacketRecived;

        private void OnOnPacketRecived(NetworkPacket packet)
        {
            var handler = OnPacketRecived;
            if (handler != null)
            {
                PacketEventArgs e = new PacketEventArgs(packet);
                handler(this, e);
            }

        }
        
    }
}
