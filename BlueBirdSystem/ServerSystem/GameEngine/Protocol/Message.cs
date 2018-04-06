namespace GameEngine.Protocol
{
    public abstract class Message
    {

        public abstract ProtocolCode Code { get; }

        public abstract byte[] ToByte();

        public abstract void FromByte(byte[] data, int startIndex);

    }
       
}
