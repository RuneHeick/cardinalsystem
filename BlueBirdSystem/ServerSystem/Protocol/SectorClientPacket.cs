using Networking.Util;
using NetworkingLayer.Util;
using Sector;
using System;

namespace Protocol
{
    public class SectorClientPacket : IClientMsg
    {
        public ulong ToSector { get; set; }
        public string Client { get; set; }

        public byte[] Data { get; set; }
        public int Index { get; set; }

        public static SectorClientPacket Create(byte[] data, int index, SocketEvent evt)
        {
            SectorClientPacket packet = new SectorClientPacket();
            if(packet.Data[index] == (byte)NetworkMessageType.SECTOR_PLAYER_MESSAGE)
            {
                packet.Index = index + (1 + 8);
                packet.Data = data;
                packet.ToSector = BitConverter.ToUInt64(data, index + 1);
                packet.Client = evt.Address;
                return packet;
            }
            throw new InvalidOperationException("The data is not correct format");
        }

        public ComposedMessage GetMessage()
        {
            ComposedMessage msg = new ComposedMessage();
            msg.Add(Data);
            return msg;
        }
    }
}
