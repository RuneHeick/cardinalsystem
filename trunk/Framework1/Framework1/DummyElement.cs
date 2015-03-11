using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace Framework1
{
    public class DummyElement:PacketElement
    {



        public override Size ExpectedSize
        {
            get { return 4; }
        }

    }
}
