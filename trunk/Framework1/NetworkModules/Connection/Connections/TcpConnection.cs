using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Helpers;
using Server3.Intercom.Network.Packets;

namespace NetworkModules.Connection.Connections
{
    internal class TcpConnection : IConnection
    {
        private readonly TcpClient _client;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private readonly List<NetworkPacket> _toBeSend = new List<NetworkPacket>(3);
        private readonly object _statusLock = new object(); 


        private byte[] _infoBuffer;
        private byte[] _packetBuffer;
        private int _rxLength = 0; 

        public IPEndPoint RemoteEndPoint { get; private set; }

        public ConnectionStatus Status
        {
            get { return _status; }
            internal set
            {
                lock (_statusLock)
                {
                    if (_status != value)
                    {
                        _status = value;
                        StatusChanged(value);
                    }
                }
            }
        }

        internal TcpConnection(TcpClient client, IPEndPoint remoteEndPoint)
        {
            _client = client;
            RemoteEndPoint = remoteEndPoint;
            Reset();
        }

        private void StatusChanged(ConnectionStatus status)
        {
            if (status == ConnectionStatus.Connected)
            {
                StartRead();
                SendQueue(); 
            }
            FireOnStatusChanged(status);
        }

        private void SendQueue()
        {
            lock (_toBeSend)
            {
                foreach (var networkPacket in _toBeSend)
                {
                    Send(networkPacket);
                }
                _toBeSend.Clear();
            }
        }

        private void StartRead()
        {
            _client.GetStream().BeginRead(_infoBuffer, 0, _infoBuffer.Length, AsyncInfoReadComplete, null);
        }

        private void AsyncInfoReadComplete(IAsyncResult ar)
        {
            try
            {
                int len = _client.GetStream().EndRead(ar);
                if (len == _infoBuffer.Length)
                {
                    int packetLen = NetworkPacket.GetPacketLength(_infoBuffer);
                    _packetBuffer = new byte[packetLen];
                    _rxLength = 0; 
                    _client.GetStream().BeginRead(_packetBuffer, 0, packetLen, AsyncPacketReadComplete, null);
                }
                else
                {
                    Status = ConnectionStatus.Disconnected;
                }
            }
            catch (Exception e)
            {
                Status = ConnectionStatus.Disconnected;
            }
        }

        private void AsyncPacketReadComplete(IAsyncResult ar)
        {
            try
            {
                int len = _client.GetStream().EndRead(ar);
                _rxLength += len;
                if (len > 0)
                {
                    if (_rxLength == _packetBuffer.Length)
                    {
                        var infoBuffer = _infoBuffer;
                        var packetBuffer = _packetBuffer;
                        Reset();
                        StartRead();  //Start Reading before using thread to handle Packet. 
                        
                        NetworkPacket recivedPacket = new NetworkPacket(infoBuffer, packetBuffer, this, PacketType.Tcp)
                        {
                            TimeStamp = DateTime.Now,
                            Address = RemoteEndPoint
                        };

                        OnOnPacketRecived(recivedPacket);
                    }
                    else
                    {
                        _client.GetStream().BeginRead(_packetBuffer, _rxLength, _packetBuffer.Length - _rxLength, AsyncPacketReadComplete, null);
                    }
                }
                else
                {
                    Status = ConnectionStatus.Disconnected;
                }
            }
            catch (Exception e)
            {
                Status = ConnectionStatus.Disconnected;
            }
        }

        private void Reset()
        {
            _infoBuffer = new byte[3];
            _rxLength = 0; 
        }

        public PacketType Supported
        {
            get { return PacketType.Tcp; }
        }

        public void Send(NetworkPacket Packet)
        {
            try
            {
                lock (_toBeSend)
                {
                    if (Status == ConnectionStatus.Disconnected)
                        return;
                    if (Status == ConnectionStatus.Connecting)
                        _toBeSend.Add(Packet);

                    if (Status == ConnectionStatus.Connected)
                    {
                        byte[] packet = Packet.Packet;
                        _client.GetStream().BeginWrite(packet, 0, packet.Length, AsyncSendComplete, null);
                    }
                }
            }
            catch (Exception)
            {
                Status = ConnectionStatus.Disconnected;
            }
        }

        private void AsyncSendComplete(IAsyncResult ar)
        {
            try
            {
                _client.GetStream().EndWrite(ar);
            }
            catch
            {
                Status = ConnectionStatus.Disconnected;
            }
        }

        public void Close()
        {
            try
            {
                if (_client.Client != null)
                {
                    _client.Client.Close();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public event PacketHandler OnPacketRecived;
      

        private void OnOnPacketRecived(NetworkPacket packet)
        {
            var handler = OnPacketRecived;
            if (handler != null)
            {
                PacketEventArgs e = new PacketEventArgs(packet);
                handler(this, e);
            }
                
        }

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
}
