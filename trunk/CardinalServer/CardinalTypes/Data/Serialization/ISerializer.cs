using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data.Serialization
{
    public interface ISerializer
    {
        string Name { get; set; }


        byte[] Serilize(object item);

        object DeSerilize(byte[] data); 

    }
}
