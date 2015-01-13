using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Server.InterCom.IPMDir
{
    public class AddressInfo
    {
        public IPEndPoint Address { get; set; }

        public ushort NetView { get; set; }

    }
}
