using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection
{
    class ProtocolManager
    {

        List<Protocol> _protocols = new List<Protocol>(); 
        
        public void PacketRecived(Connection sender, NetworkPacket packet)
        {
            lock (_protocols)
            {
                foreach (var protocol in _protocols)
                {
                    if (NeedPacket(protocol, packet))
                    {
                        protocol.HandlePacket(sender, packet);
                    }
                }
            }
        }

        private bool NeedPacket(Protocol protocol, NetworkPacket packet)
        {
            var packetTypes = packet.Elements.ConvertAll((o) => o.GetType());
            foreach (var elementType in protocol.ElementTypes)
            {
                if (packetTypes.Contains(elementType))
                    packetTypes.Remove(elementType);
                else
                    return false;
            }
            return true; 
        }


        public void Add(Protocol protocol)
        {
            lock (_protocols)
            {
                if(!_protocols.Contains(protocol))
                    _protocols.Add(protocol);
            }
        }

        public void Remove(Protocol protocol)
        {
            lock (_protocols)
            {
                _protocols.Remove(protocol);
            }
        }

    }
}
