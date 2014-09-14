using CardinalTypes.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.CardinalGroud
{
    public class CardinalGroup
    {
        public List<ICardinalType> Types { get; set; }


        public ICardinalType Parrent { get; private set; }
        public ICardinalType Chiled { get; private set; }
    }
}
