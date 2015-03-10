using System.Dynamic;

namespace NetworkModules.Connection.Packet
{
    public interface IPacketElement
    {
        ICommandId Type { get; set; }
            
        byte[] Data { get; set; }

        int Length { get; }

        bool IsFixedSize { get; }
    }
}
