using System;
using System.Net;
using System.Net.Sockets;
using Server3.Utility;

namespace Server3.Intercom.Network.NICHelpers
{
    class MulticastConnector: IConnector
    {

        private readonly UdpClient _multicastClient = new UdpClient();
        private readonly IPEndPoint _multicastAddress;

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


        public void Send(NetworkRequest request)
        {
            throw new NotImplementedException();
        }

        public void Send(Packets.NetworkPacket packet)
        {
            try
            {
                _multicastClient.BeginSend(packet.Packet, packet.Packet.Length, _multicastAddress, SendComplete, _multicastClient);
            }
            catch
            {
                multicastError();
            }
        }

        public event Action<Packets.NetworkPacket, IConnector> OnPacketRecived;
    }
}
