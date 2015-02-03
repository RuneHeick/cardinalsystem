using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NewInterCom.SharedSettings
{
    public abstract class ISetting
    {
        public string Name { get; set; }

        private byte[] data = new byte[0]; 
        public byte[] Data
        {
            get
            {
                return data; 
            }
            set
            {
                if (!isEqual(data,value))
                {
                    data = value;
                    FireOnDataChanged();
                }
            }

        }
        private void FireOnDataChanged()
        {
            Action<ISetting> Event = OnDataChanged;
            if(Event != null)
            {
                Event(this); 
            }
        }


        private static bool isEqual(byte[] a, byte[]b)
        {
            if (a.Length == b.Length)
                return a.SequenceEqual<byte>(b);
            return false;
        }
        
        public event Action<ISetting> OnDataChanged; 

    }
}
