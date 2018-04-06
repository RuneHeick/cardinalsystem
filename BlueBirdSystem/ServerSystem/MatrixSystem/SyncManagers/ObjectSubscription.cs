namespace MatrixSystem.SyncManagers
{
    public class ObjectSubscription
    {
        public uint Id { get; set; }
        public uint LastSync { get; set; } = uint.MaxValue;

        public bool IsSubscription { get; set; } = false;
        public uint LastUpdateCounter { get; set; }

        public ObjectSubscription(uint id)
        {
            Id = id;
            LastSync = 0; 
        }

    }
}
