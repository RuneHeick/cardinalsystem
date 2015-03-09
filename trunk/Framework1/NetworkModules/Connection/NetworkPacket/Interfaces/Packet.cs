using System;
using System.Net;

namespace NetworkModules.Connection.NetworkPacket
{
    public abstract class Packet
    {
        internal virtual byte[] PacketData { get; set; }

        public virtual byte Command { get; set; }

        public virtual byte Sesion { get; set; }

        public virtual bool IsResponse { get; set; }

        public virtual int PayloadLength { get; }

        public virtual int PacketLength { get; }

        public virtual byte this[int i] { get; set; }

        public DateTime TimeStamp { get; set; }

        public IPEndPoint Address { get; set; }

        public PacketType Type { get; set; }
    }

    [Flags]
    public enum PacketType
    {
        None = 0,
        Tcp = 1,
        Udp = 2,
        Multicast = 4,
    }


}
