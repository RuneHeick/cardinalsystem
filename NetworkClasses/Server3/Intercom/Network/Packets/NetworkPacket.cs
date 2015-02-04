using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network.Packets
{
    public class NetworkPacket
    {

        IPacket _packet; 

        public NetworkPacket(byte[] fullPacket)
        {
            _packet = new NetworkFullPacket(fullPacket);
        }

        public NetworkPacket(int payloadLength)
        {
            _packet = new NetworkFullPacket(payloadLength);
        }

        public NetworkPacket(byte[] info, byte[] payload)
        {
            _packet = new NetworkSegmentedPacket(info, payload);
        }

        public static int GetPacketLength(byte[] packetPart)
        {
            return packetPart[2] + ((packetPart[1] << 8) & 0x07); 
        }

        public byte[] Packet
        {
            get { return _packet.Packet; }
        }

        public byte Command
        {
            get
            {
                return _packet.Command;
            }
            set
            {
                _packet.Command = value;
            }
        }

        public byte Sesion
        {
            get
            {
                return _packet.Sesion;
            }
            set
            {
                _packet.Sesion = value;
            }
        }

        public bool IsResponse
        {
            get
            {
                return _packet.IsResponse;
            }
            set
            {
                _packet.IsResponse = value; 
            }
        }

        public int PayloadLength
        {
            get { return _packet.PayloadLength; }
        }

        public int PacketLength
        {
            get { return _packet.PacketLength; }
        }

        public byte this[int i]
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

        public DateTime TimeStamp { get; set; }

        public IPEndPoint Address { get; set;  }
    }
}
