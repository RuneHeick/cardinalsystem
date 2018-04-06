using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Protocol
{
    public class SubscriptionMessage : Message
    {

        public string PlayerID;

        public override ProtocolCode Code => ProtocolCode.SUBSCRIBE_TO_UPDATE;

        public override void FromByte(byte[] data, int startIndex)
        {
            int length = data[startIndex];
            UTF8Encoding.UTF8.GetString(data, startIndex + 1, length);
        }

        public override byte[] ToByte()
        {
            byte[] stringBytes = UTF8Encoding.UTF8.GetBytes(PlayerID, 0, PlayerID.Length);
            byte[] data = new byte[stringBytes.Length+1];

            data[0] = (byte)stringBytes.Length;
            Array.Copy(stringBytes, 0, data, 1, stringBytes.Length);
            return data;
        }
    }
}
