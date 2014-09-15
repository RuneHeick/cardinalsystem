using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    public abstract class ManagedData
    {
        public object Data { get; set; }

        public string Serilizer { get; set; }

        public long ID { get; set; }

        internal Action<ManagedData> DisposeFunction {get; set;} 

        ~ManagedData()
        {
            if (DisposeFunction != null)
                DisposeFunction(this); 
        }

    }
}
