using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Server.InterCom;
using Server3.Intercom.Network.NICHelpers;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network
{
    class NIC
    {
        //Public 
        public IPEndPoint Ip { get; set; }
        private const int RequestTTL_ms = 100;
        private const int MaxRetransmit = 5; 

        //Privates 
        private readonly IConnector[] _connectors; 

        private Dictionary<byte, RequestInfo> _requests = new Dictionary<byte, RequestInfo>();


        public NIC(IPEndPoint ip)
        {
            _connectors = new IConnector[]{new TCPConnector(ip), new UdpConnector(ip), new MulticastConnector(ip)};
            this.Ip = ip;

            foreach (var connector in _connectors)
            {
                connector.OnPacketRecived += NetworkPacketRecived;
            }
        }

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
                if (con.Support == networkPacket.Type)
                {
                    // con.Send(networkPacket);
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


                if(request.Packet.Type == PacketType.Tcp)
                    info.TimeToLive = TimeOut.Create(RequestTTL_ms, info, RequestTimeOut);
                else
                    info.TimeToLive = TimeOut.Create(RequestTTL_ms, info, RequestRetransmit);


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
