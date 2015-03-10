namespace NetworkModules.Connection.Packet
{
    public class PacketElement
    {

        public virtual ICommandId Type { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual int Length
        {
            get { return Data.Length; }
        }

        public bool IsFixedSize { get; internal set; }
    }
}
