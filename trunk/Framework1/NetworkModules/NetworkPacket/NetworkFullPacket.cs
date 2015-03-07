using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network.Packets
{
    class NetworkFullPacket: IPacket
    {

        public NetworkFullPacket(byte[] fullPacket)
        {
            Packet = fullPacket; 
        }

        public NetworkFullPacket(int length)
        {
            if(length>2041)
                throw new ArgumentOutOfRangeException();
            Packet = new byte[length+3];
            Packet[2] = (byte) length;
            Packet[1] = (byte) (length >> 8);
        }

        public byte[] Packet { get; private set; }

        public byte Command
        {
            get { return (byte)(Packet[1] >> 3); }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException("Command must be smaller than 31");
                Packet[1] = (byte)((Packet[1] & 0x07) + (value << 3));
            }
        }

        public byte Sesion
        {
            get { return (byte)(Packet[0] & 0x7F); }
            set { Packet[0] = (byte)((Packet[0]& 0x80)+(value & 0x7F)); }
        }

        public bool IsResponse
        {
            get { return (Packet[0] & 0x80) > 0; }
            set { Packet[0]  = value ? Packet[0] |= 0x80 : Packet[0] &= 0x7F; }
        }


        public byte this[int i]
        {
            get { return Packet[3 + i]; }
            set { Packet[3 + i] = value; }
        }


        public int PayloadLength
        {
            get { return Packet.Length - 3; }
        }

        public int PacketLength
        {
            get { return Packet.Length; }
        }

    }
}
