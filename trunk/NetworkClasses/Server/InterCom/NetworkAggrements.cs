using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    class NetworkAggrements
    {
        Dictionary<string, byte> Aggrements = new Dictionary<string, byte>();
        ushort AggrementsState = 0;
        byte NextFree = 0; 

        public void Add(string aggrement)
        {

        }

        public byte this[string name]
        {
            get
            {
                return Aggrements[name]; 
            }
        }


    }
}
