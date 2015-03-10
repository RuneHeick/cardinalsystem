using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connections;

namespace NetworkModules.Connection.NetworkPacket
{
    abstract class IReqest:Packet
    {
        
        internal IConnection _connector;

        public void SendReply(Packet packet)
        {
            if (_connector != null && IsResponse == false && Sesion != 0)
            {
                packet.Address = Address;
                packet.Sesion = Sesion;
                packet.IsResponse = true;
                _connector.Send(packet);
            }
            else
            {
                throw new InvalidOperationException("Cannot Replay on a Response/Signal/Self Packet");
            }
        }

    }
}
