using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkModules.Connection.Connections;
using NetworkModules.Connection.Connector;

namespace NetworkModules.Connection.Helpers
{
    class TCPConnector
    {

        private readonly TcpListener _listener;
        private readonly object _syncRoot = new object();
        private readonly IPEndPoint _meEndPoint; 

        internal TCPConnector(IPEndPoint endPoint)
        {
            _meEndPoint = endPoint;
            _listener = new TcpListener(endPoint.Address, endPoint.Port);
        }

        private void AsyncAcceptClientComplete(IAsyncResult ar)
        {
            try
            {
                var socket = _listener.EndAcceptTcpClient(ar);
                SocketId ID = new SocketId(socket);
                socket.GetStream().BeginRead(ID.Buffer, 0, 4, ReadIDCompleate, ID);
                _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
            }
            catch (Exception)
            {
                
            }
        }

        private void ReadIDCompleate(IAsyncResult ar)
        {
            SocketId ID = (SocketId)ar.AsyncState;
            try
            {
                ID.Socket.GetStream().EndRead(ar);
                int connectPort = BitConverter.ToInt32(ID.Buffer, 0);
                IPEndPoint connectionIpEndPoint = new IPEndPoint((ID.Socket.Client.RemoteEndPoint as IPEndPoint).Address,connectPort);
                OnRemoteConnectionCreated(ID.Socket, connectionIpEndPoint); 
            }
            catch (Exception)
            {
            }
        }


        protected virtual void OnRemoteConnectionCreated(TcpClient client,IPEndPoint connectSocket)
        {
            var handler = _remoteConnectionCreated;
            if (handler != null)
            {
                TcpConnection connection = new TcpConnection(client, connectSocket);
                connection.Status = ConnectionStatus.Connected;
                ConnectionEventArgs<TcpConnection> e = new ConnectionEventArgs<TcpConnection>(connection);
                handler(this, e);
            }
        }


        internal TcpConnection CreateConnection(IPEndPoint endPoint)
        {
            var client = new TcpClient();
            TcpConnection connection = new TcpConnection(client, endPoint);
            connection.Status = ConnectionStatus.Connecting;
            client.BeginConnect(endPoint.Address, endPoint.Port, AsyncConnectionComplete,
                new Tuple<TcpClient, TcpConnection>(client, connection));
            return connection;
        }

        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            var info = (Tuple<TcpClient, TcpConnection>)ar.AsyncState;
            try
            {
                info.Item1.EndConnect(ar);
                info.Item1.GetStream().Write(BitConverter.GetBytes(_meEndPoint.Port),0,4);
                info.Item2.Status = ConnectionStatus.Connected;
            }
            catch (Exception e)
            {
                info.Item2.Status = ConnectionStatus.Error;
            }
        }


        private event ConnectionHandler<TcpConnection> _remoteConnectionCreated;
        internal event ConnectionHandler<TcpConnection> RemoteConnectionCreated
        {
            add
            {
                if (_remoteConnectionCreated == null)
                {
                    _listener.Start();
                    _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
                }
                _remoteConnectionCreated += value;
            }
            remove
            {
                _remoteConnectionCreated -= value;
                if (_remoteConnectionCreated == null)
                    _listener.Stop();
            }
        }

        private class SocketId
        {
            public SocketId(TcpClient socket)
            {
                this.Socket = socket;
                Buffer = new byte[4];
            }

            public byte[] Buffer { get; set; }

            public TcpClient Socket { get; set; }
        }

        
    }
}
