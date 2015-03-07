using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class IPendPointEqualityComparer : IEqualityComparer<IPEndPoint>
    {
        public bool Equals(IPEndPoint b1, IPEndPoint b2)
        {
            return b1.Equals(b2);
        }

        public int GetHashCode(IPEndPoint bx)
        {
            return bx.GetHashCode();
        }
    }
}
