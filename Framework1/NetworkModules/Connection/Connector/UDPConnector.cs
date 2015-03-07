using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NetworkModules.Connection.Connections;
using Server.Utility;

namespace NetworkModules.Connection.Connector
{
    public class UdpConnector
    {

        readonly UdpClient _listener;
        readonly Dictionary<IPEndPoint,WeakReference<UdpConnection>> _connenections = new Dictionary<IPEndPoint, WeakReference<UdpConnection>>(new IPendPointEqualityComparer());

        public UdpConnector(IPEndPoint endPoint)
        {
            _listener = new UdpClient(endPoint);
        }

        private void StartRecive()
        {
            _listener.BeginReceive(AsyncUdpReciveComplete, null);
        }

        private void AsyncUdpReciveComplete(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] packetBytes = _listener.EndReceive(ar, ref from);
                StartRecive();

                if (0 != packetBytes.Length)
                {
                    PacketsRecived(packetBytes, from);
                }

            }
            catch (ObjectDisposedException e)
            { }
            catch (Exception e)
            {
                StartRecive();
            }
        }

        private void PacketsRecived(byte[] packetBytes, IPEndPoint from)
        {
            lock (_connenections)
            {
                if (_connenections.ContainsKey(from))
                {
                    UdpConnection con = null;
                    bool isAlive = _connenections[from].TryGetTarget(out con);
                    if (isAlive)
                        con.AddPacket(packetBytes);
                    else
                    {
                        _connenections.Remove(from);
                        PacketsRecived(packetBytes, from);
                    }
                }
                else
                {
                    UdpConnection con = new UdpConnection(this, from);
                    OnRemoteConnectionCreated(con);
                    con.AddPacket(packetBytes);
                    _connenections.Add(from, new WeakReference<UdpConnection>(con));
                }
            }
        }

        public void Send(byte[] data, IPEndPoint endPoint)
        {
            _listener.Client.SendTo(data, 0, endPoint); 
        }


        public event ConnectionHandler<UdpConnection> RemoteConnectionCreated;

        private void OnRemoteConnectionCreated(UdpConnection con)
        {
            var handler = RemoteConnectionCreated;
            if (handler != null)
            {
                var e = new ConnectionEventArgs<UdpConnection>(con);
                handler(this, e);
            }
        }

        internal void Close(UdpConnection udpConnection)
        {
            lock (_connenections)
            {
                if (_connenections.ContainsKey(udpConnection.RemoteEndPoint))
                    _connenections.Remove(udpConnection.RemoteEndPoint);
            }
        }
    }
}
