using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Util
{
    public interface ICardinalType
    {
        //Name of the property
        string IdentifyerName { get; set; }

        object Value { get; set; }

        byte[] ToByte(); 

        void FromByte(byte[] data); 
    }
}
