using System;
using NetworkModules.Connection.Helpers;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection.Connector
{

    public delegate void ConnectionHandler<T>(object sender, ConnectionEventArgs<T> e);
    public delegate void PacketHandler(object sender, PacketEventArgs e);
    public delegate void ConnectionHandler(object sender, ConnectionEventArgs e);

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

    public class ConnectionEventArgs : EventArgs
    {
        public ConnectionEventArgs(ConnectionStatus status)
        {
            Status = status;
        }

        public ConnectionStatus Status { get; private set; }
    }


}
