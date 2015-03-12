using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace Intercom.Protocols.Elements
{
    public class PCommandElement:PacketElement
    {
        public override Size ExpectedSize
        {
            get { return Size.Dynamic; }
        }
    }
}
