using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public class ITypeEqualityComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type b1, Type b2)
        {
            return b1.Name == b2.Name;
        }

        public int GetHashCode(Type bx)
        {
            return bx.GetHashCode();
        }
    }
}
