using CardinalTypes.Data;
using CardinalTypes.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.CardinalGroud
{
    public class CardinalGroup : ManagedData, ICardinalType
    {
        public List<ICardinalType> Types { get; set; }


        public ICardinalType Parrent { get; private set; }
        public ICardinalType Chiled { get; private set; }

        public string IdentifyerName { get; set; }

        public object Value { get; set; }

        public override string Serilizer { get; set; }

        public byte[] ToByte()
        {
            return null;
        }

        public void FromByte(byte[] data)
        {

        }

    }
}
