using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network
{
    public class ClientFoundEvent
    {
        public IPEndPoint Address { get; set; }

        public NIC Connector { get; set; }
    }
}
