using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;
using Server.Utility;

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
            foreach (var elementType in protocol.PacketDefinitions)
            {
                var packetTypes = packet.Elements.ConvertAll((o) => o.GetType());
                foreach (var type in elementType.ElementTypes)
                {
                    if (packetTypes.Contains(type))
                    {
                        packetTypes.Remove(type);
                    }
                    else
                    {
                        packetTypes = null; 
                        break;
                    }
                }
                if (packetTypes != null && packetTypes.Count == 0)
                    return true;
            }
            return false; 
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


        public int Count
        {
            get
            {
                lock (_protocols)
                {
                    return _protocols.Count;
                }
            }
        }
    }
}
