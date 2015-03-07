using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkModules.Connection.Helpers
{
    class TCPConnector
    {

        private readonly TcpListener _listener;
        private readonly object _syncRoot = new object();

        public TCPConnector(IPEndPoint endPoint)
        {
            _listener = new TcpListener(endPoint.Address, endPoint.Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        private void AsyncAcceptClientComplete(IAsyncResult ar)
        {
            try
            {
                var socket = _listener.EndAcceptTcpClient(ar);
                Connection.AddSocket(socket);
            }
            catch (Exception)
            {

            }
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        public Connection CreateConnection(IPEndPoint endPoint,int  maxIdleTime)
        {
            lock (_syncRoot)
            {
                Connection connection = Connection.GetConnection(endPoint, maxIdleTime);
                if (connection.Status == ConnectionStatus.Disconnected)
                {
                    connection.Status = ConnectionStatus.Connecting;
                    var client = new TcpClient();
                    client.BeginConnect(endPoint.Address, endPoint.Port, AsyncConnectionComplete,
                        new Tuple<TcpClient, Connection>(client, connection));
                }
                return connection;
            }
        }

        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            var info = (Tuple<TcpClient, Connection>)ar.AsyncState;
            try
            {
                info.Item1.EndConnect(ar);
                Connection.AddSocket(info.Item1);
                info.Item2.Status = ConnectionStatus.Connected;
            }
            catch (Exception e)
            {
                info.Item2.Status = ConnectionStatus.Disconnected;
            }

        }

    }
}
