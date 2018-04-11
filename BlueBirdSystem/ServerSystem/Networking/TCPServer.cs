using Networking;
using Networking.Util;
using NetworkingLayer.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLayer
{
    public class TCPServer : INetwork
    {
        private TcpListener _listener;
        private IPEndPoint _address { get; set; }

        private readonly object _clientsLock = new object();
        private Dictionary<string, TCPServerClient> _clients = new Dictionary<string, TCPServerClient>();

        public event SocketEventHandler OnSocketEvent;

        public TCPServer(IPEndPoint ip)
        {
            _address = ip;
            _listener = new TcpListener(ip.Address, ip.Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        public void StopServer()
        {
            _listener.Stop();
        }

        private void AsyncAcceptClientComplete(IAsyncResult ar)
        {
            try
            {
                var socket = _listener.EndAcceptTcpClient(ar);
                AddNewUser(socket);
                _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public void Send(ComposedMessage msg, string address)
        {
            throw new NotImplementedException();
        }

        private void AddNewUser(TcpClient socket)
        {
            EndPoint ip = socket.Client.RemoteEndPoint;
            string address = ip.ToString();
            TCPServerClient client = new TCPServerClient(socket, address);
            client.OnSocketEvent += HandleSocketEvents;
            lock (_clientsLock)
            {
                _clients.Add(address, client);
            }
        }

        private void HandleSocketEvents(INetwork client, SocketEvent arg)
        {
            if(arg.State == ConnectionState.Disconnected)
            {
                lock (_clientsLock)
                {
                    if (_clients.ContainsKey(arg.Address))
                    {
                        _clients.Remove(arg.Address);
                    }
                }
                client.OnSocketEvent -= HandleSocketEvents;
            }
            OnSocketEvent?.Invoke(client, arg);
        }
        
    }
}
