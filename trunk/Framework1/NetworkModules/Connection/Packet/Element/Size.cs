using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModules.Connection.Packet
{
    public class Size
    {
        private readonly int _value = 0;

        public Size(int value)
        {
            this._value = value;
        }

        public static implicit operator Size(int value)
        {
            return new Size(value);
        }

        public static implicit operator Size(byte value)
        {
            return new Size(value);
        }

        public static implicit operator int(Size integer)
        {
            return integer._value;
        }

        public static implicit operator byte(Size integer)
        {
            return (byte)integer._value;
        }

        public static int operator +(Size one, Size two)
        {
            return one._value + two._value;
        }

        public static Size operator +(int one, Size two)
        {
            return new Size(one + two._value);
        }

        public static int operator -(Size one, Size two)
        {
            return one._value - two._value;
        }

        public static Size operator -(int one, Size two)
        {
            return new Size(one - two._value);
        }

        public static Size operator +(byte one, Size two)
        {
            return new Size(one + two._value);
        }

        public static Size operator -(byte one, Size two)
        {
            return new Size(one - two._value);
        }

        public static bool operator !=(Size one, Size two)
        {
            return !one.Equals(two);
        }

        public static bool operator ==(Size one, Size two)
        {
            return one.Equals(two); 
        }


        private static readonly Size _dynamcSize = new Size(Byte.MaxValue+1);
        public static Size Dynamic { get { return _dynamcSize; } }

        public override bool Equals(object obj)
        {
            var size = obj as Size;
            if (size == null) return false;

            return (size._value == _value) || (size._value >= _dynamcSize._value && _value >= _dynamcSize._value);
        }

        public override int GetHashCode()
        {
            return _value >= _dynamcSize._value ? _dynamcSize._value : _value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
