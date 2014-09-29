using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardinalTypes.Util;

namespace CardinalTypes.Types
{
    public class CardinalInt:ICardinalType
    {
        public CardinalInt()
        {
            value_ = 0; 
        }

        public CardinalInt(int p)
        {

            value_ = p;
        }

        private int value_;

        public string IdentifyerName { get; set; }
        public object Value
        {
            get
            {
                return value_; 
            }
            set
            {
                if(value is int)
                {
                    value_ = (int)value;
                }
            }
        }

        public static CardinalInt operator +(CardinalInt first, int second)
        {
            first.Value = first.value_ + second; 
            return new CardinalInt(first.value_ + second);
        }

        public static CardinalInt operator +(int second, CardinalInt first)
        {
            first.Value = first.value_ + second;
            return new CardinalInt(first.value_ + second);
        }

        public static CardinalInt operator +(CardinalInt second, CardinalInt first)
        {
            first.Value = first.value_ + second;
            return new CardinalInt(first.value_ + second.value_);
        }

        public static CardinalInt operator -(CardinalInt first, CardinalInt second)
        {
            return new CardinalInt(first.value_ - second.value_);
        }

        public static CardinalInt operator -(CardinalInt first, int second)
        {
            first.Value = first.value_ - second;
            return new CardinalInt(first.value_ - second);
        }

        public static CardinalInt operator -(int second, CardinalInt first)
        {
            first.Value = first.value_ - second;
            return new CardinalInt(first.value_ - second);
        }

        public byte[] ToByte()
        {
            return null; 
        }

        public void FromByte(byte[] data)
        {

        }
    }
}
