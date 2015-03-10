using System;
using System.Collections.Generic;
using System.Net;

namespace NetworkModules.Connection.Packet
{
    public class NetworkPacket
    {
        private static CommandCollection _cmdCollection; 
        private PacketBuilder _builder;

        public NetworkPacket()
        {
            if(_cmdCollection == null)
                throw new NotSupportedException("CommandCollection must be set first");
            _builder = new PacketBuilder(_cmdCollection);
            Elements = new List<PacketElement>();
            Type = PacketType.Tcp;
        }

        public NetworkPacket(byte[] payload, int startindex):this()
        {
            Elements = _builder.DecomposePacket(payload, startindex); 
        }

        public List<PacketElement> Elements { get; private set; }  

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

        public static void SetCommandCollection(CommandCollection cmdCollection)
        {
            _cmdCollection = cmdCollection; 
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
