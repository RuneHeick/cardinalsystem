using System;
using System.Net;
using Server3.Intercom.Network.NICHelpers;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network
{
    public class NetworkRequest
    {

        public static NetworkPacket CreateSignal(int packetLength, PacketType type)
        {
            return new NetworkPacket(packetLength, type, true);
        }

        public static NetworkRequest Create(int packetLength, PacketType type, Action<NetworkPacket> callback ,
            Action<NetworkPacket, ErrorType> errorCallback = null)
        {
            NetworkPacket packet = new NetworkPacket(packetLength, type, callback == null);
            NetworkRequest request = new NetworkRequest()
            {
                ErrorCallbak = errorCallback,
                Packet = packet,
                ResponseCallback = callback
            };

            return request; 
        }

        private NetworkRequest()
        { }

        public NetworkPacket Packet { get; private set; }

        internal Action<NetworkPacket> ResponseCallback { get; set; }

        internal Action<NetworkPacket, ErrorType> ErrorCallbak { get; set; }
        
    }

    public enum ErrorType
    {
        TimeOut,
        Connection,
        RequestFull,
        PacketFormat
    }
}
