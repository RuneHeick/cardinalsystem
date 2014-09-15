using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    public abstract class ManagedData
    {
        public abstract byte[] Serilize();

        public abstract void Deserize(byte[] data); 

        internal Action<ManagedData> DisposeFunction {get; set;} 

        ~ManagedData()
        {
            if (DisposeFunction != null)
                DisposeFunction(this); 
        }

    }
}
