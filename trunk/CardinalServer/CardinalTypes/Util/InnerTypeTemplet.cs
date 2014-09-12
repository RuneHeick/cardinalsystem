using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Util
{
    class InnerTypeTemplet<T> where T : IComparable
    {

        public T Max { get; set; }
        public T Min { get; set; }
        public T Value { get; set; }

        public InnerTypeTemplet()
        {

        }



    }
}
