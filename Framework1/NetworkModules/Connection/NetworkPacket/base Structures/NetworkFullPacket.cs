using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.NetworkPacket;

namespace Server3.Intercom.Network.Packets
{
    class NetworkFullPacket: Packet
    {

        public NetworkFullPacket(byte[] fullPacketData)
        {
            PacketData = fullPacketData; 
        }

        public NetworkFullPacket(int length)
        {
            if(length>2041)
                throw new ArgumentOutOfRangeException();
            PacketData = new byte[length+3];
            PacketData[2] = (byte) length;
            PacketData[1] = (byte) (length >> 8);
        }

        internal override byte[] PacketData { get; set; }

        public override byte Command
        {
            get { return (byte)(PacketData[1] >> 3); }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException("Command must be smaller than 31");
                PacketData[1] = (byte)((PacketData[1] & 0x07) + (value << 3));
            }
        }

        public override byte Sesion
        {
            get { return (byte)(PacketData[0] & 0x7F); }
            set { PacketData[0] = (byte)((PacketData[0]& 0x80)+(value & 0x7F)); }
        }

        public override bool IsResponse
        {
            get { return (PacketData[0] & 0x80) > 0; }
            set { PacketData[0]  = value ? PacketData[0] |= 0x80 : PacketData[0] &= 0x7F; }
        }


        public override byte this[int i]
        {
            get { return PacketData[3 + i]; }
            set { PacketData[3 + i] = value; }
        }


        public override int PayloadLength
        {
            get { return PacketData.Length - 3; }
        }

        public override int PacketLength
        {
            get { return PacketData.Length; }
        }

    }
}
