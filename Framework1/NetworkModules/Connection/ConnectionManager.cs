using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connections;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Helpers;
using Server.Utility;

namespace NetworkModules.Connection
{
    public class ConnectionManager
    {
        readonly Dictionary<IPEndPoint, Connection>  _connections = new Dictionary<IPEndPoint, Connection>(new IPendPointEqualityComparer());
        internal readonly TCPConnector TcpConnector;
        internal readonly UdpConnector UdpConnector;

        public ConnectionManager(IPEndPoint ipEndPoint)
        {
            TcpConnector = new TCPConnector(ipEndPoint);
            UdpConnector = new UdpConnector(ipEndPoint);

            TcpConnector.RemoteConnectionCreated += NewTcpConnectionAdded;
            UdpConnector.RemoteConnectionCreated += NewUdpConnectionAdded; 

        }


        private void NewTcpConnectionAdded(object sender, ConnectionEventArgs<TcpConnection> e)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(e.Connection.RemoteEndPoint))
                {
                    _connections[e.Connection.RemoteEndPoint].AddTcp(e.Connection);
                }
                else
                {
                    Connection connection = new Connection(this, e.Connection.RemoteEndPoint);
                    connection.AddTcp(e.Connection);
                    _connections.Add(e.Connection.RemoteEndPoint, connection);
                    OnRemoteConnectionCreated(connection);
                }
            }
        }

        private void NewUdpConnectionAdded(object sender, ConnectionEventArgs<UdpConnection> e)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(e.Connection.RemoteEndPoint))
                {
                    _connections[e.Connection.RemoteEndPoint].AddUdp(e.Connection);
                }
                else
                {
                    Connection connection = new Connection(this, e.Connection.RemoteEndPoint);
                    _connections.Add(e.Connection.RemoteEndPoint, connection);
                    connection.AddUdp(e.Connection);
                    OnRemoteConnectionCreated(connection);
                }
            }
        }


        public Connection GetConnection(IPEndPoint remoteEndPoint, int inactiveMaxTime)
        {
            lock (_connections)
            {
                if (_connections.ContainsKey(remoteEndPoint))
                    return _connections[remoteEndPoint];

                var udpConnection = UdpConnector.GetConnection(remoteEndPoint);
                var tcpConnection = TcpConnector.CreateConnection(remoteEndPoint, inactiveMaxTime);

                Connection connection = new Connection(tcpConnection, udpConnection, this, inactiveMaxTime);
                _connections.Add(remoteEndPoint, connection);
                return connection;
            }
        }


        public event ConnectionHandler<Connection> RemoteConnectionCreated;

        private void OnRemoteConnectionCreated(Connection connection)
        {
            var handler = RemoteConnectionCreated;
            if (handler != null)
            {
                var e = new ConnectionEventArgs<Connection>(connection);
                handler(this, e);
            }
        }


    }
}
