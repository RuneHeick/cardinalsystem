using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data.Serialization
{
    public interface ISerializer
    {
        public string Name { get; set; }


        public byte[] Serilize(object item);

        public object DeSerilize(byte[] data); 

    }
}
