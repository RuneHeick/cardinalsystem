using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class IPEqualityComparer : IEqualityComparer<IPAddress>
    {
        public bool Equals(IPAddress b1, IPAddress b2)
        {
            return b1.Equals(b2);
        }

        public int GetHashCode(IPAddress bx)
        {
            return bx.GetHashCode();
        }
    }
}
