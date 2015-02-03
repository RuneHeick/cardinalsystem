using System;
using System.Net;
using Server3.Intercom.Network.NICHelpers;

namespace Server3.Intercom.Network
{
    public class NetworkRequest
    {
        public IPEndPoint Address { get; set; }

        public NetworkPacket Packet { get; set; }

        public Action<byte, byte[]> ResponseCallback { get; set; }
        
    }
}
