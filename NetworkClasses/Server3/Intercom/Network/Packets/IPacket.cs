using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network.Packets
{
    interface IPacket
    {
        byte[] Packet { get; }

        byte Command { get; set; }

        byte Sesion { get; set; }

        bool IsResponse { get; set; }

        int PayloadLength { get; }

        int PacketLength { get; }

        byte this[int i] { get; set; }

        DateTime TimeStamp { get; set; }

    }
}
