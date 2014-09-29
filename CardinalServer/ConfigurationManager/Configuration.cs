using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationManager
{
    public class Configuration
    {
        public string Name {get; set;}
        string value_;

        public string Group { get; set; }

        public void SetValue(string Value)
        {
            value_ = Value;
        }

        public void SetValue(int Value)
        {
            try
            {
                value_ = Convert.ToString(Value);
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
                value_ = Convert.ToString(Value);
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
                value_ = Convert.ToString(Value);
            }
            catch
            {
                value_ = null;
            }
        }

        public override string ToString()
        {
            return value_;
        }

        public int ToInt()
        {
            if (value_ == null)
                return 0;
            try
            {
                return Convert.ToInt32(value_);
            }
            catch
            {
                value_ = "0";
                return 0;
            }
        }

        public long ToLong()
        {
            if (value_ == null)
                return 0;
            try
            {
                return Convert.ToInt64(value_);
            }
            catch
            {
                value_ = "0";
                return 0;
            }
        }

        public int ToByte()
        {
            if (value_ == null)
                return 0;
            try
            {
                return Convert.ToByte(value_);
            }
            catch
            {
                value_ = "0";
                return 0;
            }
        }


        private Action<Configuration> CloseOption { get; set; }


        public Configuration(Action<Configuration> closeAction)
        {
            CloseOption = closeAction; 

        }

        ~Configuration()
        {
            if (CloseOption != null)
            {
                CloseOption(this); 
            }
        }


    }
}
