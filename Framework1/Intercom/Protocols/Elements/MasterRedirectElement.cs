using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace Intercom.Protocols.Elements
{
    public class MasterRedirectElement:PacketElement
    {

        IPEndPoint Master
        {
            get
            {
                byte[] addressBytes = new byte[4];
                Array.Copy(Data,0,addressBytes,0,4);
                IPAddress address = new IPAddress(addressBytes);
                int port = BitConverter.ToInt32(Data, 5);
                return new IPEndPoint(address,port);
            }
            set
            {
                byte[] addressBytes = value.Address.GetAddressBytes();
                Array.Copy(addressBytes,0,Data,0,4);
                byte[] port = BitConverter.GetBytes(value.Port);
                Array.Copy(port, 0, Data, 5, 4);
            }
        }

        public override Size ExpectedSize
        {
            get { return 8; }
        }
    }
}
