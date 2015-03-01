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

        byte[] value_ { get; set; }
       

        public void SetValue(string Value)
        {
            value_ = UTF8Encoding.UTF8.GetBytes(Value);
        }

        public void SetValue(int Value)
        {
            try
            {
                value_ = BitConverter.GetBytes(Value);
            }
            catch
            {
                value_ = null;
            }
        }

        public void SetValue(long Value)
        {
            try
            {
                value_ = BitConverter.GetBytes(Value);
            }
            catch
            {
                value_ = null;
            }
        }

        public void SetValue(byte Value)
        {
            try
            {
                value_ = new byte[] { Value };
            }
            catch
            {
                value_ = null;
            }
        }

        public void SetValue(byte[] Value)
        {
            value_ = Value;
        }

        public override string ToString()
        {
            return UTF8Encoding.UTF8.GetString(value_);
        }

        public int ToInt()
        {
            if (value_ == null)
                return 0;
            try
            {
                return BitConverter.ToInt32(value_, 0);
            }
            catch
            {
                return 0;
            }
        }

        public long ToLong()
        {
            if (value_ == null)
                return 0;
            try
            {
                return BitConverter.ToInt64(value_, 0);
            }
            catch
            {
                return 0;
            }
        }

        public byte ToByte()
        {
            if (value_ == null)
                return 0;
            try
            {
                return value_[0];
            }
            catch
            {
                return 0;
            }
        }

        public byte[] ToArray()
        {
            if (value_ == null)
                return null;
            try
            {
                return value_;
            }
            catch
            {
                return null;
            }
        }
    }
}
