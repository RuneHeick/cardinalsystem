using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    public interface IData
    {
        bool IsAlive { get; set; }

        byte[] Serilize();

        void Deserize(byte[] data); 

    }
}
