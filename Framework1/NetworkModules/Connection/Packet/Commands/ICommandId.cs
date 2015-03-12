using System;

namespace NetworkModules.Connection.Packet.Commands
{
    interface ICommandId
    {
        byte Command { get; set; }

        string Name { get; }

        PacketElement CreateElement();

        Type ElementType { get; }

    }

}
