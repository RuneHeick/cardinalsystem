using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.NetworkPacket;

namespace Server3.Intercom.Network.Packets
{
    class NetworkSegmentedPacket:Packet
    {

        private byte[] _info;
        private byte[] _packet;

        public NetworkSegmentedPacket(byte[] info, byte[] packet)
        {
            _info = info;
            _packet = packet; 
        }

        internal override byte[] PacketData
        {
            get 
            {
                byte[] fullPacket = new byte[_info.Length + _packet.Length];
                Array.Copy(_info,0,fullPacket,0,_info.Length);
                Array.Copy(_packet, 0, fullPacket, _info.Length, _packet.Length);
                return fullPacket;
            }
        }

        public override byte Command
        {
            get { return (byte)(_info[1] >> 3); }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException("Command must be smaller than 31");
                _info[1] = (byte)((_info[1] & 0x07) + (value << 3));
            }
        }

        public override byte Sesion
        {
            get { return (byte)(_info[0] & 0x7F); }
            set { _info[0] = (byte)((_info[0] & 0x80) + (value & 0x7F)); }
        }

        public override bool IsResponse
        {
            get { return (_info[0] & 0x80) > 0; }
            set { _info[0] = value ? _info[0] |= 0x80 : _info[0] &= 0x7F; }
        }

        public override int PayloadLength
        {
            get { return _packet.Length; }
        }

        public override int PacketLength
        {
            get { return PayloadLength + _info.Length; }
        }

        public override byte this[int i]
        {
            get
            {
                return _packet[i];
            }
            set
            {
                _packet[i] = value;
            }
        }

    }
}
