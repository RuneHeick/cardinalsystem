using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.Util
{
    public class ComposedMessageStream
    {
        private byte[] _message;

        public byte[] Message => _message;
        public int CurrentIndex { get; set; } 

        public ComposedMessageStream(ComposedMessage message)
        {
            if(message.Message.Count == 1)
                _message = message.Message[0];
            else
            {
                _message = new byte[message.Length];
                int index = 0; 
                foreach(var bytes in message.Message)
                {
                    Array.Copy(bytes, 0, _message, index, bytes.Length);
                    index += bytes.Length;
                }
            }
            CurrentIndex = 0; 
        }

        public ComposedMessageStream(byte[] message)
        {
            _message = message;
            CurrentIndex = 0;
        }

    }
}
