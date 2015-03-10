namespace NetworkModules.Connection.Packet
{
    public class PacketElement : IPacketElement
    {
        private ICommandId _type;

        public PacketElement()
        {
        }


        public PacketElement(byte[] data, ICommandId command)
        {
            Type = command;
            Data = data;
        }

        public virtual ICommandId Type
        {
            get { return _type; }
            set
            {
                _type = value;
                IsFixedSize = (value.Length != CommandId<PacketElement>.Dynamic);
            }
        }

        public virtual byte[] Data { get; set; }

        public virtual int Length
        {
            get { return Data.Length; }
        }

        public bool IsFixedSize { get; internal set; }
    }
}
