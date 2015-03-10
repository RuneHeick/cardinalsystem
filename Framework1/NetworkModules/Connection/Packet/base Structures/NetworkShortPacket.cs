using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.NetworkPacket;

namespace Server3.Intercom.Network.Packets
{
    class NetworkShortPacket: Packet
    {
        private readonly byte[] _packetData; 

        public NetworkShortPacket(int payloadLength, bool isSignal)
        {
            _packetData = new byte[payloadLength + (isSignal ? 1 : 2)];
            HasSession = !isSignal;
        }

        public NetworkShortPacket(byte[] packetData)
        {
            _packetData = packetData; 
        }

        internal override byte[] PacketData
        {
            get { return _packetData; }
        }

        public override byte Command
        {
            get { return (byte)(_packetData[0] >> 3); }
            set
            {
                if (value > 0x1F)
                    throw new ArgumentOutOfRangeException("Command must be smaller than 31");
                _packetData[0] = (byte)((_packetData[0] & 0x07) + (value << 3));
            }
        }

        public override byte Sesion
        {
            get
            {
                if (HasSession)
                    return (byte)(_packetData[1] & 0x7F);
                return 0; 
            }
            set
            {
                if (HasSession)
                    _packetData[1] = (byte)((_packetData[1] & 0x80) + (value & 0x7F)); 
            }
        }

        public bool HasSession
        {
            get { return (_packetData[0] & 0x01) > 0;  }
            set { _packetData[0] |= (byte) (value ? 0x01 : 0x00); }
        }

        public override bool IsResponse
        {
            get
            {
                if (HasSession)
                    return (_packetData[1] & 0x80) > 0;
                return false;
            }
            set
            {
                if (HasSession)
                    _packetData[1] = value ? _packetData[1] |= 0x80 : _packetData[1] &= 0x7F; 
            }
        }

        public override int PayloadLength
        {
            get { return PacketData.Length -(HasSession ? 2 : 1); }
        }

        public override int PacketLength
        {
            get { return _packetData.Length; }
        }

        public override byte this[int i]
        {
            get
            {
                return _packetData[i + (HasSession ? 2 : 1)];
            }
            set
            {
                _packetData[i+(HasSession ? 2 : 1)] = value;
            }
        }
    }
}
