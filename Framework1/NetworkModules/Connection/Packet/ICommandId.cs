namespace NetworkModules.Connection.Packet
{
    public interface ICommandId
    {
        byte Command { get; }

        string Description { get; }

        int Length { get; }

        PacketElement CreateElement();
    }

}
