using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public class TCPConnector : IConnector
    {
        private const int ConnectionMaxIdleTime_ms = 30000;
        private const int RequestTTL_ms = 5000;

        private readonly List<ClientInfo> _connectedClients = new List<ClientInfo>();

        private readonly TcpListener _listener;

        public IPEndPoint Address { get; private set; }

        public int OpenConnections
        {
            get { return _connectedClients.Count; }
        }

        public TCPConnector(IPEndPoint ip)
        {
            Address = ip;
            _listener = new TcpListener(ip.Address, ip.Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        public void Stop()
        {
            _listener.Stop();
            lock (_connectedClients)
            {
                foreach (var client in _connectedClients)
                {
                    client.Client.Close();
                }
            }
        }

        private void AsyncAcceptClientComplete(IAsyncResult ar)
        {
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
            try
            {
                var socket = _listener.EndAcceptTcpClient(ar);
                var info = AddClient(socket, ((IPEndPoint) socket.Client.RemoteEndPoint).Address, false);
                StartRead(info);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Send(NetworkRequest request)
        {
            List<RequestInfo> rq = AddRequestBank(request.Packet.Address.Address, request);

            lock (_connectedClients)
            {
                var connection = _connectedClients.FirstOrDefault(
                    (o) => o.Address.Equals(request.Packet.Address.Address));
                if (connection == null)
                    StartConnect(request);
                else if (connection.Client.Connected && connection.IsConnecting == false)
                    StartSendReqest(connection, rq);

            }
        }

        private void StartConnect(NetworkRequest request)
        {
            lock (_connectedClients)
            {
                var connection = _connectedClients.FirstOrDefault(
                    (o) => o.Address.Equals(request.Packet.Address.Address));
                if (connection == null)
                {
                    ClientInfo info = AddClient(new TcpClient(), request.Packet.Address.Address, true);
                    info.Client.BeginConnect(request.Packet.Address.Address, request.Packet.Address.Port,
                        AsyncConnectionComplete, info);
                }
            }
        }

        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                info.Client.EndConnect(ar);
                info.IsConnecting = false;
                StartRead(info);
            }
            catch (Exception e)
            {
                CloseClientInfo(info);
            }


            lock (_requestBank)
            {
                if (_requestBank.ContainsKey(info.Address))
                {
                    StartSendReqest(info,
                        _requestBank[info.Address]);
                }
            }
        }

        private void StartRead(ClientInfo info)
        {
            info.Client.GetStream()
                .BeginRead(info.InfoBuffer, 0, info.InfoBuffer.Length, AsyncInfoReadComplete, info);
        }

        private void AsyncInfoReadComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                int len = info.Client.GetStream().EndRead(ar);
                if (len == info.InfoBuffer.Length)
                {
                    int packetLen = NetworkPacket.GetPacketLength(info.InfoBuffer);
                    info.PacketBuffer = new byte[packetLen];
                    info.Client.GetStream().BeginRead(info.PacketBuffer, 0, packetLen, AsyncPacketReadComplete, info);
                }
                else
                {
                    CloseClientInfo(info);
                }
            }
            catch (Exception e)
            {
                CloseClientInfo(info);
            }
        }

        private void AsyncPacketReadComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                int len = info.Client.GetStream().EndRead(ar);
                if (len == info.PacketBuffer.Length)
                {
                    var infoBuffer = info.InfoBuffer;
                    var packetBuffer = info.PacketBuffer;
                    var RecivedTime = DateTime.Now;

                    info.LastTouch = RecivedTime;
                    info.BufferReset();

                    StartRead(info); //Start Reading before using thread to handle Packet. 

                    NetworkPacket recivedPacket = new NetworkPacket(infoBuffer, packetBuffer, this, PacketType.Tcp)
                    {
                        TimeStamp = RecivedTime,
                        Address = info.Client.Client.RemoteEndPoint as IPEndPoint
                    };

                    PacketRecived(recivedPacket);
                }
                else
                {
                    CloseClientInfo(info);
                }
            }
            catch (Exception)
            {
                CloseClientInfo(info);
            }
        }

        private void PacketRecived(NetworkPacket packet)
        {
            try
            {
                if (packet.IsResponse)
                {
                    RequestInfo rq = null;
                    lock (_requestBank)
                    {
                        if (_requestBank.ContainsKey(packet.Address.Address))
                        {
                            var list = _requestBank[packet.Address.Address];
                            rq = list.FirstOrDefault((o) => o.Request.Packet.Sesion == packet.Sesion);
                            if (rq != null)
                            {
                                list.Remove(rq);
                                rq.TTL.Calcel();
                            }
                        }
                    }
                    if (rq != null)
                    {
                        rq.Request.ResponseCallback(packet);
                    }
                    else
                    {
                        //rougePacket
                    }
                }
                else
                {
                    // For thread safty
                    Action<NetworkPacket, IConnector> Event = OnPacketRecived;
                    if (Event != null)
                        Event(packet, this);
                }
            }
            catch
            {
                // Packet handle error 
            }
        }

        private void StartSendReqest(ClientInfo stream, List<RequestInfo> requestInfos)
        {
            lock (_requestBank)
            {
                for (int i = 0; i < requestInfos.Count; i++)
                {
                    if (requestInfos[i].IsSend == false)
                    {
                        byte[] packet = requestInfos[i].Request.Packet.Packet;
                        stream.Client.GetStream()
                            .BeginWrite(packet, 0, packet.Length,
                                AsyncWriteComplete, stream);
                        requestInfos[i].IsSend = true;
                        if (requestInfos[i].Request.Packet.Sesion == 0)
                        {
                            requestInfos.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }

        private void AsyncWriteComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                info.Client.GetStream().EndWrite(ar);
                info.LastTouch = DateTime.Now;
            }
            catch (Exception)
            {
                CloseClientInfo(info);
            }
        }

        private void CloseClientInfo(ClientInfo info)
        {
            lock (_connectedClients)
            {
                _connectedClients.Remove(info);
                info.CloseTimer.Calcel();
                ClearRequestBank(info.Address);
                if (info.Client != null)
                {
                    info.Client.Close();
                }
            }
        }

        private ClientInfo AddClient(TcpClient socket, IPAddress address, bool isConnecting)
        {
            ClientInfo info = new ClientInfo()
            {
                Address = address,
                Client = socket,
                LastTouch = DateTime.Now,
                IsConnecting = isConnecting,
            };
            info.CloseTimer = TimeOut.Create(30000, info, CloseClientInfo);
            lock (_connectedClients)
                _connectedClients.Add(info);

            return info;
        }

        #region RequestBank

        private void RemoveRequestInfo(RequestInfo rq)
        {
            lock (_requestBank)
            {
                var ip = rq.Request.Packet.Address.Address;
                if (_requestBank.ContainsKey(ip))
                {
                    var list = _requestBank[ip];
                    lock (list)
                        list.Remove(rq);
                }
            }
        }

        private void ClearRequestBank(IPAddress address)
        {
            lock (_requestBank)
            {
                if (_requestBank.ContainsKey(address))
                {
                    var list = _requestBank[address];
                    if (list != null)
                    {
                        lock (list)
                        {
                            foreach (RequestInfo rq in list)
                            {
                                if (rq.Request.ErrorCallbak != null)
                                    rq.Request.ErrorCallbak(rq.Request.Packet, ErrorType.Connection);
                            }
                        }
                    }
                    _requestBank.Remove(address);
                }
            }
        }

        private List<RequestInfo> AddRequestBank(IPAddress address, NetworkRequest request)
        {
            RequestInfo rqi = null;
            List<RequestInfo> rq;
            lock (_requestBank)
            {
                if (!_requestBank.ContainsKey(request.Packet.Address.Address))
                {
                    _requestBank.Add(request.Packet.Address.Address, new List<RequestInfo>(1));
                }

                rq = _requestBank[request.Packet.Address.Address];
                if (null == rq.FirstOrDefault((o) => o.Request == request))
                {
                    try
                    {
                        rqi = new RequestInfo(request, rq);
                        lock (rq)
                            rq.Add(rqi);
                        rqi.TTL = TimeOut.Create(RequestTTL_ms, rqi, RequestTimeOutHandler);
                    }
                    catch (Exception)
                    {
                        if (request.ErrorCallbak != null)
                            request.ErrorCallbak(request.Packet, ErrorType.RequestFull);
                    }
                }
            }

            return rq;
        }

        private void RequestTimeOutHandler(RequestInfo obj)
        {
            RemoveRequestInfo(obj);
            if (obj.Request.ErrorCallbak != null)
                obj.Request.ErrorCallbak(obj.Request.Packet, ErrorType.TimeOut);
        }

        #endregion


        public event Action<NetworkPacket, IConnector> OnPacketRecived;


        private class ClientInfo
        {
            public ClientInfo()
            {
                BufferReset();
            }

            public IPAddress Address { get; set; }

            public byte[] InfoBuffer { get; set; }

            public byte[] PacketBuffer { get; set; }

            public bool IsConnecting { get; set; }

            public TcpClient Client { get; set; }

            private DateTime _lastTouch;

            public TimeOut CloseTimer { get; set; }

            public DateTime LastTouch
            {
                get { return _lastTouch; }
                set
                {
                    _lastTouch = value;
                    if (CloseTimer != null)
                        CloseTimer.Touch();
                }
            }

            public void BufferReset()
            {
                InfoBuffer = new byte[3];
                PacketBuffer = null;
            }

            public PacketType Support
            {
                get { return PacketType.Tcp; }
            }

        }

    }
}
