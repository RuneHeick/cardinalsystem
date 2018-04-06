using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.Util
{
    public class ComposedMessage
    {
        private int _length = 0; 
        private List<byte[]> _messageParts = new List<byte[]>();

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public List<byte[]> Message
        {
            get
            {
                return _messageParts;
            }
        }

        public void AddToFront(byte[] message)
        {
            _messageParts.Insert(0, message);
            _length += message.Length;
        }


        public void Add(byte[] message)
        {
            _messageParts.Add(message);
            _length += message.Length;
        }

     
    }
}
