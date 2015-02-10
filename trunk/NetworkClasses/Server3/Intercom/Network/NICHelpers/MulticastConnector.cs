using System;
using System.Net;
using System.Net.Sockets;
using Server3.Intercom.Network.Packets;
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
            if (!range.IsInRange(multicastAddress.Address))
                throw new InvalidOperationException("Not a Multicast Address");

            _multicastAddress = multicastAddress;
            _multicastClient.JoinMulticastGroup(_multicastAddress.Address);
            _multicastClient.MulticastLoopback = true;

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
                    NetworkPacket nPacket = new NetworkPacket(data, this, PacketType.Multicast);
                    nPacket.Address = end;
                    nPacket.TimeStamp = DateTime.Now; 

                    try
                    {
                        // For thread safty
                        Action<NetworkPacket, IConnector> Event = OnPacketRecived;
                        if (Event != null)
                            Event(nPacket, this);

                    }
                    catch (Exception)
                    {
                        // ignored
                    }
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

        public PacketType Supported
        {
            get { return PacketType.Multicast;}
        }

        public void Send(NetworkPacket packet)
        {
            _multicastClient.BeginSend(packet.Packet, packet.Packet.Length, _multicastAddress, AsyncSendComplete, null);
        }

        private void AsyncSendComplete(IAsyncResult ar)
        {
            try
            {
                _multicastClient.EndSend(ar);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public event Action<Packets.NetworkPacket, IConnector> OnPacketRecived;

        public void Close()
        {
            try
            {
                _multicastClient.Close();
            }
            catch (Exception)
            {
                //ignore 
            }

        }

    }
}
