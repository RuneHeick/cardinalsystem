using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkModules.Connection.Packet.Commands;

namespace NetworkModules.Connection.Packet
{
    public class NetworkPacket
    {
        private PacketBuilder _builder;

        public NetworkPacket()
        {
            _builder = new PacketBuilder();
            Elements = new List<PacketElement>();
            Type = PacketType.Tcp;
        }

        public NetworkPacket(byte[] payload, int startindex) : this()
        {
            Elements = _builder.DecomposePacket(payload, startindex);
        }

        public List<PacketElement> Elements { get; private set; }

        public List<T> GetNetworkElements<T>()
        {
            return Elements.OfType<T>().ToList();
        }

        public void Add(PacketElement element)
        {
            _builder.Add(element);
            Elements.Add(element);
        }

        public void Remove(PacketElement element)
        {
            _builder.Remove(element);
            Elements.Remove(element);
        }

        public DateTime TimeStamp { get; set; }

        public IPEndPoint EndPoint { get; set; }

        public PacketType Type { get; set; }

        internal byte[] FullPacket
        {
            get { return _builder.CreatePackets(); }
        }
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
