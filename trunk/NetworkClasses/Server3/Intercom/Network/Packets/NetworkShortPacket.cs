using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network.Packets
{
    class NetworkShortPacket: IPacket
    {
        private readonly byte[] _packet; 

        public NetworkShortPacket(int payloadLength, bool isSignal)
        {
            _packet = new byte[payloadLength + (isSignal ? 1 : 2)];
            HasSession = !isSignal;
        }

        public NetworkShortPacket(byte[] packet)
        {
            _packet = packet; 
        }

        public byte[] Packet
        {
            get { return _packet; }
        }

        public byte Command
        {
            get { return (byte)(_packet[0] >> 3); }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException("Command must be smaller than 31");
                _packet[0] = (byte)((_packet[0] & 0x07) + (value << 3));
            }
        }

        public byte Sesion
        {
            get
            {
                if (HasSession)
                    return (byte)(_packet[1] & 0x7F);
                return 0; 
            }
            set
            {
                if (HasSession)
                    _packet[1] = (byte)((_packet[1] & 0x80) + (value & 0x7F)); 
            }
        }

        public bool HasSession
        {
            get { return (_packet[0] & 0x01) > 0;  }
            set { _packet[0] |= (byte) (value ? 0x01 : 0x00); }
        }

        public bool IsResponse
        {
            get
            {
                if (HasSession)
                    return (_packet[1] & 0x80) > 0;
                return false;
            }
            set
            {
                if (HasSession)
                    _packet[1] = value ? _packet[1] |= 0x80 : _packet[1] &= 0x7F; 
            }
        }

        public int PayloadLength
        {
            get { return Packet.Length -(HasSession ? 2 : 1); }
        }

        public int PacketLength
        {
            get { return _packet.Length; }
        }

        public byte this[int i]
        {
            get
            {
                return _packet[i + (HasSession ? 2 : 1)];
            }
            set
            {
                _packet[i+(HasSession ? 2 : 1)] = value;
            }
        }
    }
}
