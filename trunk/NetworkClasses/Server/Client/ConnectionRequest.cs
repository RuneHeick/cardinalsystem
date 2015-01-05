using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Client
{
    class ConnectionRequest: IClient
    {
        public IPAddress IP
        {
            get { throw new NotImplementedException(); }
        }

    }
}
