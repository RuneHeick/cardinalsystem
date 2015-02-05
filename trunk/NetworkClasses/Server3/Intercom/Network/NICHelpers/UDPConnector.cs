using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public class UdpConnector: IConnector
    {
        private const int RequestTTL_ms = 5000;
        public IPEndPoint Me { get; private set; }
        readonly UdpClient _listener;

        public UdpConnector(IPEndPoint address)
        {
            Me = address;
            _listener = new UdpClient(Me);
            StartRecive();
        }

        public void Stop()
        {
            try
            {
                _listener.Close();
                SenderInfo.Dispose();
            }
            catch (Exception)
            {
                //ignore 
            }
            
        }

        private void AsyncUdpInfoReciveComplete(IAsyncResult ar)
        {
            ReciverInfo info = (ReciverInfo) ar.AsyncState;
            info.TimeStamp = DateTime.Now;

            try
            {
                IPEndPoint from = new IPEndPoint(0,0);
                info.PacketBuffer = _listener.EndReceive(ar, ref from);
                info.Address = from;
                StartRecive();

                if (0 != info.PacketBuffer.Length)
                {
                    PacketsRecived(info);
                }
                else
                {
                    StartRecive();
                }
            }
            catch(ObjectDisposedException e)
            { }
            catch(Exception e)
            {
                StartRecive();
            }
        }


        private void PacketsRecived(ReciverInfo info)
        {
            try
            {
                NetworkPacket nPacket = new NetworkPacket(info.PacketBuffer, this, PacketType.Udp);
                nPacket.Address = info.Address;
                nPacket.TimeStamp = info.TimeStamp;
                if (nPacket.IsResponse)
                {
                    var item = SenderInfo.GetSenderInfo(info.Address);
                    item.HandleNetworkRq(nPacket);
                }
                else
                {
                    // For thread safty
                    Action<NetworkPacket, IConnector> Event = OnPacketRecived;
                    if (Event != null)
                        Event(nPacket, this);
                }
            }
            catch(Exception)
            {

            }
        }

        private void StartRecive()
        {
            var item = new ReciverInfo();
            _listener.BeginReceive(AsyncUdpInfoReciveComplete, item);
        }


        public void Send(NetworkRequest request)
        {
            var info = SenderInfo.GetSenderInfo(request.Packet.Address);

            //is Request
            if (request.ResponseCallback != null)
            {
                byte[] packet = info.PrepareSendRq(request, RequestTTL_ms);
                if (packet != null)
                {
                    _listener.BeginSend(packet, packet.Length, request.Packet.Address, AsyncCallbackComplete,null);
                }
            }
            //is Signal
            else
            {
                _listener.BeginSend(request.Packet.Packet, request.Packet.Packet.Length, request.Packet.Address, AsyncCallbackComplete, null);
            }
        }

        private void AsyncCallbackComplete(IAsyncResult ar)
        {
            try
            {
                _listener.EndSend(ar);
            }
            catch
            {
                // ignored
            }
        }

        public void Send(NetworkPacket packet)
        {
            NetworkRequest item = new NetworkRequest()
            {
                Packet = packet
            };
            Send(item);
        }

        public event Action<NetworkPacket, IConnector> OnPacketRecived;

        private class ReciverInfo
        {
            public IPEndPoint Address { get; set; }

            public byte[] PacketBuffer { get; set; }

            public DateTime TimeStamp { get; set; }

        }

        private class SenderInfo
        {
            private static Dictionary<IPAddress, SenderInfo> _addressInfo = new Dictionary<IPAddress, SenderInfo>(new IPEqualityComparer());

            public static SenderInfo GetSenderInfo(IPEndPoint address)
            {
                lock (_addressInfo)
                {
                    if (!_addressInfo.ContainsKey(address.Address))
                        _addressInfo.Add(address.Address, new SenderInfo());

                    return _addressInfo[address.Address];
                }
            }

            public static void Dispose()
            {
                lock (_addressInfo)
                {
                    _addressInfo.Clear();
                }
            }

            private SenderInfo()
            {
                SendRequests = new List<RequestInfo>();
            }

            private List<RequestInfo> SendRequests { get; set; }

            public byte[] PrepareSendRq(NetworkRequest request, uint timeout)
            {
                if (request.Packet.PayloadLength > 1000)
                {
                    if (request.ErrorCallbak != null)
                        request.ErrorCallbak(request.Packet, ErrorType.PacketFormat);
                    return null;
                }

                bool gotSession = false;
                lock (SendRequests)
                {
                    for (byte session = 1; session < 0x7F; session++)
                    {
                        var version = SendRequests.FirstOrDefault((o) => o.Request.Packet.Sesion == session);
                        if (version == null)
                        {
                            request.Packet.Sesion = session;
                            gotSession = true;
                            break;
                        }
                    }
                }
                if (!gotSession)
                {
                    if (request.ErrorCallbak != null)
                        request.ErrorCallbak(request.Packet, ErrorType.RequestFull);
                    return null;
                }

                RequestInfo info = new RequestInfo()
                {
                    Request = request
                };
                info.TTL = TimeOut.Create(timeout, info, RequestTimeOutInfo);
                
                lock (SendRequests)
                    SendRequests.Add(info);

                return request.Packet.Packet;
            }

            public void HandleNetworkRq(NetworkPacket Response)
            {
                if(!Response.IsResponse) return;
                RequestInfo rqInfo;
                lock (SendRequests)
                {
                    rqInfo = SendRequests.FirstOrDefault((o) => o.Request.Packet.Sesion == Response.Sesion);
                    if (rqInfo != null)
                        SendRequests.Remove(rqInfo); 
                }

                if (rqInfo != null)
                {
                    rqInfo.TTL.Calcel();
                    if (rqInfo.Request.ResponseCallback != null)
                        rqInfo.Request.ResponseCallback(Response);
                }
            }

            private void Error(RequestInfo rqInfo, ErrorType error)
            {
                lock (SendRequests)
                    SendRequests.Remove(rqInfo);

                if(rqInfo.TTL != null)
                    rqInfo.TTL.Calcel();

                if (rqInfo.Request.ErrorCallbak != null)
                    rqInfo.Request.ErrorCallbak(rqInfo.Request.Packet, error);
            }

            private void RequestTimeOutInfo(RequestInfo rqInfo)
            {
                Error(rqInfo, ErrorType.TimeOut);
            }


            private class RequestInfo
            {
                public NetworkRequest Request { get; set; }

                public TimeOut TTL { get; set; }
            }

        }

    }

    
}
