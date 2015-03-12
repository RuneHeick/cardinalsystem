using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NetworkModules.Connection.Connections;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection
{
    public class Connection
    {
        private  TcpConnection _tcpConnection = null;
        private  UdpConnection _udpConnection = null;
        private readonly ConnectionManager _manager;
        private readonly int _inactiveMaxTime;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private readonly object _tcpLock = new object();
        private readonly object _udpLock = new object();

        public IPEndPoint RemoteEndPoint { get; internal set; }

        private ProtocolManager Protocols { get; set; }

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
                    FireOnStatusChanged(value);
                }
            }
        }
        
        internal Connection(TcpConnection tcpConnection, UdpConnection udpConnection, ConnectionManager manager, int inactiveMaxTime)
        {
            _manager = manager;
            _inactiveMaxTime = inactiveMaxTime;
            RemoteEndPoint = udpConnection.RemoteEndPoint; 
            Setup();
  
            AddTcp(tcpConnection);
            AddUdp(udpConnection);
            
        }

        internal Connection(ConnectionManager manager, IPEndPoint remoteEndPoint)
        {
            _inactiveMaxTime = Timeout.Infinite;
            _manager = manager;
            RemoteEndPoint = remoteEndPoint; 
            Setup();
        }

        ~Connection()
        {
            Close();
        }

        public void Setup()
        {
            Protocols = new ProtocolManager();
        }

        #region Protocol 
        public void Add(Protocol protocol)
        {
            bool add = Protocols.Count == 0;

            Protocols.Add(protocol);
            
            if (add)
            {
                if (_tcpConnection != null)
                    _tcpConnection.OnPacketRecived += OnPacketRecived;
                if (_udpConnection != null)
                    _udpConnection.OnPacketRecived += OnPacketRecived;
            }
        }

        public void Remove(Protocol protocol)
        {
            Protocols.Remove(protocol);

            if (Protocols.Count == 0)
            {
                if (_tcpConnection != null)
                    _tcpConnection.OnPacketRecived -= OnPacketRecived;
                if (_udpConnection != null)
                    _udpConnection.OnPacketRecived -= OnPacketRecived;
            }
        }

        private void OnPacketRecived(object sender, PacketEventArgs e)
        {
            Protocols.PacketRecived(this, e.Packet);
        }

        #endregion

        #region SocketOperations
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
                    _udpConnection.OnPacketRecived -= OnPacketRecived;
                    _udpConnection.Close();
                }
                
        }

        private void SendTcp(NetworkPacket packet)
        {
            lock (_tcpLock)
            {
                if (_tcpConnection == null || _tcpConnection.Status == ConnectionStatus.Disconnected)
                    AddTcp(_manager.TcpConnector.CreateConnection(RemoteEndPoint, _inactiveMaxTime));
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

        #endregion

        #region AddSocket
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
                    if(Protocols.Count>0)
                        _tcpConnection.OnPacketRecived += OnPacketRecived;
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
                    if (Protocols.Count > 0)
                        _udpConnection.OnPacketRecived += OnPacketRecived;
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
                    _tcpConnection.OnPacketRecived -= OnPacketRecived;
                    _tcpConnection = null;
                }
                Status = e.Status;
            }

        }

        #endregion

        public event ConnectionHandler OnStatusChanged;

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
