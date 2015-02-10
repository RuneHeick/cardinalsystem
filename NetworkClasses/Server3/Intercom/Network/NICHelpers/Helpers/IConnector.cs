using System;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public interface IConnector
    {
        PacketType Supported { get;  }
        
        void Send(NetworkPacket request);

        void Close();

        event Action<NetworkPacket, IConnector> OnPacketRecived;
    }
}