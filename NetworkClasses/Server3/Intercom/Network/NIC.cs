using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Server.InterCom;
using Server3.Intercom.Network.NICHelpers;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network
{
    public class NIC
    {
        //Public 
        public IPEndPoint Ip { get; set; }
        public const int RequestTTL_ms = 100;
        public const int MaxRetransmit = 5;
        public IPEndPoint[] KnownEndPoints
        {
            get
            {
                IPEndPoint[] item;
                lock (_knownEndPoints)
                    item = _knownEndPoints.Values.ToArray();
                return item; 
            }
        }


        //Privates 
        private readonly IConnector[] _connectors; 
        private readonly Dictionary<byte, RequestInfo> _requests = new Dictionary<byte, RequestInfo>();
        private readonly Dictionary<IPAddress, IPEndPoint> _knownEndPoints = new Dictionary<IPAddress, IPEndPoint>();

        
        public NIC(IPEndPoint ip)
        {
            _connectors = new IConnector[]{new TCPConnector(ip), new UdpConnector(ip), new MulticastConnector(new IPEndPoint(IPAddress.Parse("239.0.0.1"),2020))};
            this.Ip = ip;

            foreach (var connector in _connectors)
            {
                connector.OnPacketRecived += NetworkPacketRecived;
            }

            SendPortMessage(false);
            EventBus.Subscribe<NetworkRequest>(HandleNewRequest); 
        }

        private void HandleNewRequest(NetworkRequest obj)
        {
            Send(obj);
        }

        #region Send/Recive
        private void NetworkPacketRecived(NetworkPacket packet, IConnector connector)
        {
            if (packet.IsResponse)
            {
                RequestInfo info = null; 
                lock (_requests)
                {
                    if (_requests.ContainsKey(packet.Sesion))
                    {
                        info = _requests[packet.Sesion];
                        if (info.MultibleRecivers == false)
                        {
                            info.TimeToLive.Calcel();
                            _requests.Remove(packet.Sesion);
                        }
                    }
                }
                if (info != null)
                {
                    info.Request.ResponseCallback(packet); 
                }
            }
            else
            {
                // awnser multicast with UDP 
                if (packet.Type == PacketType.Multicast)
                {

                    if (packet.Command == (byte) Intercom.InterComCommands.PortMessage)
                    {
                        HandlePortMessage(packet);
                        return;
                    }

                    if (_knownEndPoints.ContainsKey(packet.Address.Address))
                    {
                        packet._connector = _connectors.FirstOrDefault((o) => o.Supported == PacketType.Udp);
                        packet.Address = _knownEndPoints[packet.Address.Address];
                    }
                    else
                    {
                        SendPortMessage(false);
                    }
                }

                EventBus.Publich<NetworkPacket>(packet, false);
            }
        }

        public void Send(NetworkPacket packet)
        {
            packet.Sesion = 0;
            packet.IsResponse = false;
            SendPacket(packet); 
        }

        public void Send(NetworkRequest request)
        {
            // Is Request 
            if (request.ResponseCallback != null)
            {
                PrepareReqest(request);
            }
            //Is Signal 
            else
            {
                Send(request.Packet);
            }
        }

        private void SendPacket(NetworkPacket networkPacket)
        {
            foreach (var con in _connectors)
            {
                if (con.Supported == networkPacket.Type)
                {
                    con.Send(networkPacket);
                    return;
                }
            }
        }

        private void PrepareReqest(NetworkRequest request)
        {
            lock (_requests)
            {
                byte session = 1;
                var gotSession = false;
                for (; session < 0x7F; session++)
                {
                    if(!_requests.ContainsKey(session))
                    {
                        request.Packet.Sesion = session;
                        gotSession = true;
                        break;
                    }
                }

                // If no more sessions 
                if (!gotSession)
                {
                    if (request.ErrorCallbak != null)
                        request.ErrorCallbak(request.Packet, ErrorType.RequestFull);
                    return;
                }

                request.Packet.Sesion = session;
                request.Packet.IsResponse = false;
                RequestInfo info = new RequestInfo()
                {
                    Request = request,
                };


                if (request.Packet.Type == PacketType.Tcp)
                    info.TimeToLive = TimeOut.Create(RequestTTL_ms, info, RequestTimeOut);
                else
                {
                    if (request.Packet.Type == PacketType.Multicast)
                        info.MultibleRecivers = true;
                    info.TimeToLive = TimeOut.Create(RequestTTL_ms, info, RequestRetransmit);
                }
                    


                _requests.Add(session, info);
            }
            SendPacket(request.Packet);
        }

        private void RequestRetransmit(RequestInfo obj)
        {
            if (obj.TransmisionTimes > MaxRetransmit)
                RequestTimeOut(obj);
            else
            {
                obj.TransmisionTimes++;
                SendPacket(obj.Request.Packet);
                obj.TimeToLive = TimeOut.Create(RequestTTL_ms, obj, RequestRetransmit);
            }
        }

        private void RequestTimeOut(RequestInfo obj)
        {
            lock (_requests)
            {
                _requests.Remove(obj.Request.Packet.Sesion);
            }
            if (obj.Request.ErrorCallbak != null)
            {
                obj.Request.ErrorCallbak(obj.Request.Packet, ErrorType.TimeOut);
            }
        }

        #endregion

        public void Close()
        {
            foreach (var con in _connectors)
            {
                con.Close();
            }
        }

        #region PortMessage
        private void HandlePortMessage(NetworkPacket packet)
        {
            // is Request 
            if (packet[0] == 0)
            {
                SendPortMessage(true);
            }
            byte[] data = new byte[4];
            NetworkPacket.Copy(data, 0, packet, 1, 4);
            AddToKnownAddresses(packet.Address.Address, BitConverter.ToInt32(data, 0));
        }

        private void AddToKnownAddresses(IPAddress address, int port)
        {
            lock (_knownEndPoints)
            {
                if (_knownEndPoints.ContainsKey(address))
                    _knownEndPoints[address].Port = port;
                else
                {
                    _knownEndPoints.Add(address, new IPEndPoint(address, port));
                    ClientFoundEvent Event = new ClientFoundEvent()
                    {
                        Address = _knownEndPoints[address],
                        Connector = this
                    };
                    EventBus.Publich(Event);
                }
                   

            }
        }

        private void SendPortMessage(bool isReply)
        {
            NetworkPacket portMessage = new NetworkPacket(5, PacketType.Multicast, true);
            NetworkPacket.Copy(portMessage, 1, BitConverter.GetBytes(Ip.Port), 0, 4);
            portMessage[0] = (byte)(isReply ? 1 : 0); 

            Send(portMessage);
        }

        #endregion
    }

    internal class RequestInfo
    {
        private bool _multibleRecivers = false;
        public NetworkRequest Request { get; set; }

        public TimeOut TimeToLive { get; set; }

        public byte Session
        {
            get
            {
                if (Request != null)
                    return Request.Packet.Sesion;
                return 0;
            }
        }

        public int TransmisionTimes = 1; 

        public bool MultibleRecivers
        {
            get { return _multibleRecivers; }
            set { _multibleRecivers = value; }
        }
    }
}
