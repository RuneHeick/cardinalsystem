using System;
using NetworkModules.Connection.Packet;

namespace Intercom.Protocols.Elements
{
    public class HelloElement : PacketElement
    {
        public int PerformanceIndex
        {
            get { return BitConverter.ToInt32(Data,0); }
            set { Data = BitConverter.GetBytes(value); }
        }

        public override Size ExpectedSize
        {
            get { return 4; }
        }
    }
}
