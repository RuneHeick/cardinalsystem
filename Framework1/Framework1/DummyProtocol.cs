using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkModules.Connection;
using NetworkModules.Connection.Packet;

namespace Framework1
{
    public class DummyProtocol:Protocol
    {

        public DummyProtocol(Type elementType)
        {
            ElementTypes = new List<Type>(1);
            ElementTypes.Add(elementType);
            ElementTypes.Add(elementType);
            ElementTypes.Add(elementType);
        }


        public override List<Type> ElementTypes { get; protected set; }

        public override void HandlePacket(Connection from, NetworkPacket networkPacket)
        {
            
        }
    }
}
