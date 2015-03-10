using System;
using System.Net;
using System.Net.Sockets;
using NetworkModules.Connection.Connections;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection
{
    public class Connection:IConnection
    {
        private  TcpConnection _tcpConnection = null;
        private  UdpConnection _udpConnection = null;
        private readonly ConnectionManager _manager;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private readonly object _tcpLock = new object();
        private readonly object _udpLock = new object();

        public IPEndPoint RemoteEndPoint { get; internal set; }

        public PacketType Supported
        {
            get
            {
                PacketType type = PacketType.None;
                lock (_tcpLock)
                if (_tcpConnection != null)
                    type |= _tcpConnection.Supported;
                lock (_udpLock)
                if (_udpConnection != null)
                    type |= _udpConnection.Supported;
                return type;
            }
        }

        public ConnectionStatus Status
        {
            get { return _status; }
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged(value);
                }
            }
        }
        
        internal Connection(TcpConnection tcpConnection, UdpConnection udpConnection, ConnectionManager manager)
        {
            RemoteEndPoint = udpConnection.RemoteEndPoint; 
            AddTcp(tcpConnection);
            AddUdp(udpConnection);
            _manager = manager;
        }


        internal Connection(ConnectionManager manager, IPEndPoint remoteEndPoint)
        {
            RemoteEndPoint = remoteEndPoint; 
            _manager = manager;
        }

        public void Send(NetworkPacket packet)
        {
            switch (packet.Type)
            {
               case PacketType.Tcp:
                    SendTcp(packet);
                    break;
               case PacketType.Udp:
                    SendUdp(packet);
                    break;
            }
        }

        public void Close()
        {
            lock (_tcpLock)
                if (_tcpConnection != null)
                {
                    _tcpConnection.Close();
                }

            lock (_udpLock)
                if (_udpConnection != null)
                {
                    _udpConnection.OnPacketRecived -= PacketRecived;
                    _udpConnection.Close();
                }
                
        }

        private void PacketRecived(object sender, PacketEventArgs e)
        {
            OnOnPacketRecived(e);
        }

        private void SendTcp(NetworkPacket packet)
        {
            lock (_tcpLock)
            {
                if (_tcpConnection == null || _tcpConnection.Status == ConnectionStatus.Disconnected)
                    AddTcp(_manager.TcpConnector.CreateConnection(RemoteEndPoint));
                packet.EndPoint = RemoteEndPoint;
                _tcpConnection.Send(packet);
            }
        }

        private void SendUdp(NetworkPacket packet)
        {
            lock (_udpLock)
            {
                if (_udpConnection == null)
                    _udpConnection = _manager.UdpConnector.GetConnection(RemoteEndPoint);
                packet.EndPoint = RemoteEndPoint;
                _udpConnection.Send(packet);
            }
        }

        internal void AddTcp(TcpConnection connection)
        {
            lock (_tcpLock)
            {
                if (_tcpConnection == null || (_tcpConnection.Status == ConnectionStatus.Error) ||
                    (_tcpConnection.Status == ConnectionStatus.Disconnected))
                {
                    _tcpConnection = connection;
                    Status = _tcpConnection.Status;
                    _tcpConnection.OnStatusChanged += TcpStatusChanged;
                    _tcpConnection.OnPacketRecived += PacketRecived;
                }
            }
        }

        internal void AddUdp(UdpConnection connection)
        {
            lock (_udpLock)
            {
                if (_udpConnection == null)
                {
                    _udpConnection = connection;
                    _udpConnection.OnPacketRecived += PacketRecived;
                }
            }
        }

        private void TcpStatusChanged(object sender, ConnectionEventArgs e)
        {
            lock (_tcpLock)
            {
                if (e.Status == ConnectionStatus.Disconnected || e.Status == ConnectionStatus.Error)
                {
                    _tcpConnection.OnStatusChanged -= TcpStatusChanged;
                    _tcpConnection.OnPacketRecived -= PacketRecived;
                    _tcpConnection = null;
                }
                Status = e.Status;
            }

        }

        public event ConnectionHandler OnStatusChanged;

        public event PacketHandler OnPacketRecived;

        private void OnOnPacketRecived(PacketEventArgs e)
        {
            var handler = OnPacketRecived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void StatusChanged(ConnectionStatus value)
        {
            FireOnStatusChanged(value);
        }

        private void FireOnStatusChanged(ConnectionStatus status)
        {
            var handler = OnStatusChanged;
            if (handler != null)
            {
                var e = new ConnectionEventArgs(status);
                handler(this, e);
            }

        }

    }


    public enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected,
        Error
    }



}
