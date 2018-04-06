using Networking.Util;
using Sector;
using System;

namespace Protocol
{
    public class SectorSectorPacket : ISectorMsg
    {
        public ulong FromSector { get; set; }
        public ulong ToSector { get; set; }

        public byte[] Data { get; set; }
        public int Index { get; set; }


        public static SectorSectorPacket Create(byte[] data, int index)
        {
            SectorSectorPacket packet = new SectorSectorPacket();
            if(packet.Data[index] == (byte)NetworkMessageType.SECTOR_SECTOR_MESSAGE)
            {
                packet.Index = index + (1 + 8 + 8);
                packet.Data = data;
                packet.ToSector = BitConverter.ToUInt64(data, index + 1);
                packet.FromSector = BitConverter.ToUInt64(data, index + 1+ 8);
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
