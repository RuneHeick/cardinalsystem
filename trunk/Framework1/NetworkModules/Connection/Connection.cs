using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connections;
using NetworkModules.Connection.Connector;
using Server3.Intercom.Network.Packets;

namespace NetworkModules.Connection.Helpers
{
    class Connection:IConnection
    {
        private TcpConnection _tcpConnection;
        private UdpConnection _udpConnection;


        public PacketType Supported
        {
            get
            {
                PacketType type = PacketType.None;
                if (_tcpConnection != null)
                    type |= _tcpConnection.Supported;
                if (_udpConnection != null)
                    type |= _udpConnection.Supported;
                return type;
            }
        }

        public void Send(NetworkPacket packet)
        {
            switch (packet.Type)
            {
               case PacketType.Tcp:
                    _tcpConnection.Send(packet);
                    break;
               case PacketType.Udp:
                    _udpConnection.Send(packet);
                    break;
            }
        }

        public void Close()
        {
            if(_tcpConnection != null)
                _tcpConnection.Close();
            if(_udpConnection != null)
                _udpConnection.Close();
        }

        public event PacketHandler OnPacketRecived;

        protected virtual void OnOnPacketRecived(NetworkPacket packet)
        {
            var handler = OnPacketRecived;
            if (handler != null)
            {
                PacketEventArgs e = new PacketEventArgs(packet);
                handler(this, e);
            }
        }
    }


    public enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected
    }



}
