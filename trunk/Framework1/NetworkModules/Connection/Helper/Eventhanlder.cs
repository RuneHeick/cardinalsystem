using System;
using Server3.Intercom.Network.Packets;

namespace NetworkModules.Connection.Connector
{

    public delegate void ConnectionHandler<T>(object sender, ConnectionEventArgs<T> e);
    public delegate void PacketHandler(object sender, PacketEventArgs e);

    public class ConnectionEventArgs<T> : EventArgs
    {
        public ConnectionEventArgs(T connection)
        {
            Connection = connection; 
        }

        public T Connection { get; private set; }
    }

    public class PacketEventArgs : EventArgs
    {
        public PacketEventArgs(NetworkPacket packet)
        {
            Packet = packet;
        }

        public NetworkPacket Packet { get; private set; }
    }

}
