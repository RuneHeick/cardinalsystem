using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection.Protocol
{
    public abstract class Protocol
    {
        public abstract List<Type> ElementTypes { get; }

        public abstract void HandlePacket(Connection from , NetworkPacket networkPacket);

    }
}
