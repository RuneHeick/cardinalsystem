using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection
{
    public abstract class Protocol
    {

        public Protocol()
        {
            PacketDefinitions = new List<PacketDefinition>();
        }

        public List<PacketDefinition> PacketDefinitions { get; protected set; }

        public abstract void HandlePacket(Connection from , NetworkPacket networkPacket);

    }
}
