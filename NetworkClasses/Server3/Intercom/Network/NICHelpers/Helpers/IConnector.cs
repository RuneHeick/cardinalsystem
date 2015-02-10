using System;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public interface IConnector
    {
        PacketType Support { get;  }

        void Send(NetworkRequest request);

        event Action<NetworkPacket, IConnector> OnPacketRecived;
    }
}