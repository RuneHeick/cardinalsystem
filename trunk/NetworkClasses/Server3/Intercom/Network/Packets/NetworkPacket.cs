using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Network.NICHelpers;

namespace Server3.Intercom.Network.Packets
{
    public class NetworkPacket
    {

        IPacket _packet;
        internal IConnector _connector;

        internal NetworkPacket(byte[] fullPacket, IConnector connector, PacketType type)
        {
            Type = type;
            if (type == PacketType.Tcp)
                _packet = new NetworkFullPacket(fullPacket);
            else
                _packet = new NetworkShortPacket(fullPacket);
            _connector = connector;
        }


        public static void Copy(NetworkPacket destination, int destinationindex, byte[] source, int sourceindex, int len)
        {
            for (int i = 0; i < len; i++)
            {
                destination[i + destinationindex] = source[i + sourceindex];
            }
        }

        public static void Copy(byte[] destination, int destinationindex, NetworkPacket source, int sourceindex, int len)
        {
            for (int i = 0; i < len; i++)
            {
                destination[i + destinationindex] = source[i + sourceindex];
            }
        }

        public static void Copy(NetworkPacket destination, int destinationindex, NetworkPacket source, int sourceindex, int len)
        {
            for (int i = 0; i < len; i++)
            {
                destination[i + destinationindex] = source[i + sourceindex];
            }
        }

        internal NetworkPacket(int payloadLength, PacketType type, bool isSignal = false)
        {
            Type = type; 
            if(type == PacketType.Tcp)
                _packet = new NetworkFullPacket(payloadLength);
            else
            {
                _packet = new NetworkShortPacket(payloadLength, isSignal);
            }
        }


        internal NetworkPacket(byte[] info, byte[] payload, IConnector connector, PacketType type)
        {
            _packet = new NetworkSegmentedPacket(info, payload);
            _connector = connector;
            Type = type; 
        }

        public void SendReply(NetworkPacket packet)
        {
            if (_connector != null && IsResponse == false && Sesion != 0)
            {
                packet.Address = Address;
                packet.Sesion = Sesion;
                packet.IsResponse = true;
                _connector.Send(packet);
            }
            else
            {
                throw new InvalidOperationException("Cannot Replay on a Response/Signal/Self Packet");
            }
        }

        internal static int GetPacketLength(byte[] packetPart)
        {
            return packetPart[2] + ((packetPart[1] & 0x07) << 8); 
        }

        internal byte[] Packet
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

        internal byte Sesion
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

        public PacketType Type { get; set; }
    }

    public enum PacketType
    {
        Tcp,
        Udp, 
        Multicast,
    }
}
