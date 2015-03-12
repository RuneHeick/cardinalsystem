using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Helpers;
using NetworkModules.Connection.Packet;
using Server.InterCom;

namespace NetworkModules.Connection.Connections
{
    internal class TcpConnection : IConnection
    {
        private readonly TcpClient _client;
        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private readonly List<NetworkPacket> _toBeSend = new List<NetworkPacket>(3);
        private readonly object _statusLock = new object();


        private readonly object _timerLock = new object();
        private TimeOut _inactiveTimer; 

        private byte[] _infoBuffer;
        private byte[] _packetBuffer;
        private int _rxLength;

        private bool _readStarted = false;

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

        internal TcpConnection(TcpClient client, IPEndPoint remoteEndPoint, int inactiveMaxTime)
        {
            _client = client;
            if(inactiveMaxTime != Timeout.Infinite)
                _inactiveTimer = TimeOut.Create((uint)inactiveMaxTime, this, ConnectionTimeOut);
            RemoteEndPoint = remoteEndPoint;
            Reset();
        }

        #region InactiveTimerdel

        private static void ConnectionTimeOut(TcpConnection obj)
        {
            lock (obj._timerLock)
            {
                obj._inactiveTimer = null;
                obj.Close();
            }
        }

        private void Kick()
        {
            lock(_timerLock)
                if(_inactiveTimer != null)
                    _inactiveTimer.Touch();
        }

        private void Cancel()
        {
            lock (_timerLock)
                if (_inactiveTimer != null)
                    _inactiveTimer.Calcel();
        }

        #endregion 

        #region Recive

        private void StartRead()
        {
            if (_status == ConnectionStatus.Connected && !_readStarted && onPacketRecived != null)
            {
                _readStarted = true; 
                _client.GetStream().BeginRead(_infoBuffer, 0, 1, AsyncInfoOneReadComplete, null);
            }
        }

        private void AsyncInfoOneReadComplete(IAsyncResult ar)
        {
            try
            {
                int len = _client.GetStream().EndRead(ar);
                if (len == 1)
                {
                    if ((_infoBuffer[0] & 0x80) > 0)
                    {
                        _client.GetStream().BeginRead(_infoBuffer, 1, 1, AsyncInfoTwoReadComplete, null);
                    }
                    else
                    {
                        _infoBuffer[1] = _infoBuffer[0];
                        _infoBuffer[0] = 0;
                        int packetLen = PacketBuilder.GetPacketLength(_infoBuffer);
                        _packetBuffer = new byte[packetLen];
                        _rxLength = 0;
                        _client.GetStream().BeginRead(_packetBuffer, 0, packetLen, AsyncPacketReadComplete, null);
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

        private void AsyncInfoTwoReadComplete(IAsyncResult ar)
        {
            try
            {
                int len = _client.GetStream().EndRead(ar);
                if (len == 1)
                {
                    int packetLen = PacketBuilder.GetPacketLength(_infoBuffer);
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
                        _readStarted = false; 
                        Reset();
                        StartRead();  //Start Reading before using thread to handle Packet. 

                        NetworkPacket recivedPacket = new NetworkPacket(packetBuffer, 0)
                        {
                            TimeStamp = DateTime.Now,
                            EndPoint = RemoteEndPoint,
                            Type = PacketType.Tcp
                        };
                        Kick();
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

        #endregion


        #region Send

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

        public void Send(NetworkPacket Packet)
        {
            Kick();
            if (Status == ConnectionStatus.Disconnected)
                return;
            byte[] packet = Packet.FullPacket;

            try
            {
                lock (_toBeSend)
                {
                    if (Status == ConnectionStatus.Connecting)
                        _toBeSend.Add(Packet);

                    if (Status == ConnectionStatus.Connected)
                    {
                        _client.GetStream().BeginWrite(packet, 0, packet.Length, AsyncSendComplete, null);
                    }
                }
            }
            catch (Exception e)
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


        #endregion

        private void StatusChanged(ConnectionStatus status)
        {
            switch(status)
            {
                case ConnectionStatus.Connected:
                StartRead();
                SendQueue(); 
                Kick();
                    break;
                case ConnectionStatus.Disconnected:
                case ConnectionStatus.Error:
                    Cancel(); 
                    break;
            }

            FireOnStatusChanged(status);
        }

        private void Reset()
        {
            _infoBuffer = new byte[2];
            _rxLength = 0; 
        }

        public PacketType Supported
        {
            get { return PacketType.Tcp; }
        }


        public void Close()
        {
            try
            {
                Cancel();
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

        private event PacketHandler onPacketRecived;
        public event PacketHandler OnPacketRecived
        {
            add
            {
                onPacketRecived += value;
                if(!_readStarted)
                    StartRead();
            }
            remove { onPacketRecived -= value; }
        }
        

        private void OnOnPacketRecived(NetworkPacket packet)
        {
            var handler = onPacketRecived;
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
