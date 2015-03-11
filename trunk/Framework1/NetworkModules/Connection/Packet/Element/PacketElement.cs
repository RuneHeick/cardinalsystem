namespace NetworkModules.Connection.Packet
{
    public abstract class PacketElement
    {

        public virtual byte[] Data { get; set; }

        public virtual int Length
        {
            get { return Data.Length; }
        }

        public abstract Size ExpectedSize { get; }

    }
}
