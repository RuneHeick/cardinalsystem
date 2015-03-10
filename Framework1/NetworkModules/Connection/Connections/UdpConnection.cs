using System;
using System.Net;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection.Connections
{
    public class UdpConnection : IConnection
    {
        private UdpConnector uDPConnector;
        public IPEndPoint RemoteEndPoint { get; private set; }
        internal UdpConnection(UdpConnector udpConnector, System.Net.IPEndPoint endPoint)
        {
            this.uDPConnector = udpConnector;
            this.RemoteEndPoint = endPoint;
        }

        internal void AddPacket(byte[] packetBytes)
        {
            NetworkPacket packet = new NetworkPacket(packetBytes, PacketBuilder.IndexserSize)
            {
                TimeStamp = DateTime.Now,
                EndPoint = RemoteEndPoint,
                Type =PacketType.Udp
            };
            OnOnPacketRecived(packet);
        }

        public PacketType Supported
        {
            get { return PacketType.Udp;}
        }

        public void Send(NetworkPacket packet)
        {
            uDPConnector.Send(packet.FullPacket, packet.EndPoint); 
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
