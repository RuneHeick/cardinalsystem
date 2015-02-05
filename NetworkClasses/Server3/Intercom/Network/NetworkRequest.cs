using System;
using System.Net;
using Server3.Intercom.Network.NICHelpers;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network
{
    public class NetworkRequest
    {
        public NetworkPacket Packet { get; set; }

        public Action<NetworkPacket> ResponseCallback { get; set; }

        public Action<NetworkPacket, ErrorType> ErrorCallbak { get; set; }
        
    }

    public enum ErrorType
    {
        TimeOut,
        Connection,
        RequestFull,
        PacketFormat
    }
}
