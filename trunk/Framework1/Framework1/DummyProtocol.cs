using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModules.Connection;
using NetworkModules.Connection.Packet;

namespace Framework1
{
    public class DummyProtocol : Protocol
    {

        public DummyProtocol(Type elementType)
        {
            PacketDefinition packetDefinition = new PacketDefinition();
            packetDefinition.ElementTypes.Add(elementType);
            packetDefinition.ElementTypes.Add(elementType);
            packetDefinition.ElementTypes.Add(elementType);
            PacketDefinitions.Add(packetDefinition);

        }

        public override void HandlePacket(Connection from, NetworkPacket networkPacket)
        {
            Console.WriteLine("Packet Recived");
        }

    }
}
