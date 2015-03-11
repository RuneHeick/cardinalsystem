using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;
using NetworkModules.Connection.Packet.Commands;

namespace NetworkModules.DataCentricSocket.Elements
{
    public class SocketHeader : PacketElement
    {
        public SocketHeader()
        {
        }

        public byte Session
        {
            get { return Data[0]; }
            set
            {
                if (Data != null)
                    Data[0] = value;
            }
        }

        public override Size ExpectedSize
        {
            get { return 4; }
        }
    }
}
