using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Network.Packets;
using Server3.Utility;

namespace Server3.Intercom.Network.NICHelpers
{
    class MulticastConnector: IConnector
    {
        private const int TransmissionInterval_ms = 20;
        private const int TransmissionMaxRetry = 5;

        private readonly UdpClient _multicastClient = new UdpClient();
        private readonly IPEndPoint _multicastAddress;

        private static Dictionary<byte, SenderInfo> _sendRequests = new Dictionary<byte, SenderInfo>();

        public MulticastConnector(IPEndPoint multicastAddress)
        {
            IPAddressRange range = new IPAddressRange(IPAddress.Parse("224.0.0.0"),IPAddress.Parse("239.255.255.255"));
            if (range.IsInRange(_multicastAddress.Address))
                throw new InvalidOperationException("Not a Multicast Address");

            _multicastAddress = multicastAddress;
            _multicastClient.JoinMulticastGroup(_multicastAddress.Address);
            _multicastClient.MulticastLoopback = false;

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, _multicastAddress.Port);
            _multicastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _multicastClient.Client.Bind(localEp);
            _multicastClient.BeginReceive(MulticastRecived, _multicastClient); 
        }

        private void MulticastRecived(IAsyncResult ar)
        {
            try
            {
                IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _multicastClient.EndReceive(ar, ref end);
                _multicastClient.BeginReceive(MulticastRecived, _multicastClient); 
                if (data.Length != 0)
                {
                    
                } 
            }
            catch
            {
                multicastError(); 
            }
            
        }

        private void multicastError()
        {
            
        }

        private void SendComplete(IAsyncResult ar)
        {
            try
            {
                _multicastClient.EndSend(ar);
            }
            catch
            {
                 multicastError(); 
            }
        }

        public PacketType Support
        {
            get { return PacketType.Multicast;}
        }

        public void Send(NetworkRequest request)
        {
            throw new NotImplementedException();
        }

        public event Action<Packets.NetworkPacket, IConnector> OnPacketRecived;

        private class ReciverInfo
        {
            public IPEndPoint Address { get; set; }

            public byte[] PacketBuffer { get; set; }

            public DateTime TimeStamp { get; set; }

        }

        private class SenderInfo
        {
            public TimeOut TTL { get; set; }

            public NetworkRequest Request { get; set; }

            public byte Session
            {
                get
                {
                    if (Request != null)
                        return Request.Packet.Sesion;
                    return 0; 
                }
            }

        }


    }
}
