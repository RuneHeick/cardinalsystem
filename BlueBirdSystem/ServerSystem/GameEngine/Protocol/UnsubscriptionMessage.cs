using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Protocol
{
    public class UnsubscriptionMessage : Message
    {

        public string PlayerID;

        public override ProtocolCode Code => ProtocolCode.UNSUBSCRIBE_TO_UPDATE;

        public override void FromByte(byte[] data, int startIndex)
        {
            
        }

        public override byte[] ToByte()
        {
            return new byte[1];
        }
    }
}
