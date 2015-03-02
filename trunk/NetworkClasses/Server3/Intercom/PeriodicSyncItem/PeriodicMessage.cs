using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.PeriodicSyncItem
{
    public class PeriodicMessage
    {

        public PeriodicMessage(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        private byte[] _value;
       

        public void SetValue(int Value)
        {
            try
            {
                _value = BitConverter.GetBytes(Value);
            }
            catch
            {
                _value = null;
            }
        }

        public void SetValue(long Value)
        {
            try
            {
                _value = BitConverter.GetBytes(Value);
            }
            catch
            {
                _value = null;
            }
        }

        public void SetValue(byte Value)
        {
            try
            {
                _value = new byte[] { Value };
            }
            catch
            {
                _value = null;
            }
        }

        public void SetValue(byte[] Value)
        {
            _value = Value;
        }

        public override string ToString()
        {
            return UTF8Encoding.UTF8.GetString(_value);
        }

        public int ToInt()
        {
            if (_value == null)
                return 0;
            try
            {
                return BitConverter.ToInt32(_value, 0);
            }
            catch
            {
                return 0;
            }
        }

        public long ToLong()
        {
            if (_value == null)
                return 0;
            try
            {
                return BitConverter.ToInt64(_value, 0);
            }
            catch
            {
                return 0;
            }
        }

        public byte ToByte()
        {
            if (_value == null)
                return 0;
            try
            {
                return _value[0];
            }
            catch
            {
                return 0;
            }
        }

        public byte[] ToArray()
        {
            if (_value == null)
                return null;
            try
            {
                return _value;
            }
            catch
            {
                return null;
            }
        }
    }
}
