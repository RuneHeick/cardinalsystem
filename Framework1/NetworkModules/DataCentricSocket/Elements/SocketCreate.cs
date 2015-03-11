using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;
using NetworkModules.Connection.Packet.Commands;

namespace NetworkModules.DataCentricSocket.Elements
{
    class SocketCreate : PacketElement
    {

        public override Size ExpectedSize
        {
            get { return Size.Dynamic; }
        }
    }
}
