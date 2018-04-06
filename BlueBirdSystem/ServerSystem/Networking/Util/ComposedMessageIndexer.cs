using System;
using System.Collections.Generic;
using System.Text;

namespace Networking.Util
{
    class ComposedMessageIndexer
    {
        public ComposedMessage Message { get; set; }

        public int Index { get; set; }

        public ComposedMessageIndexer(ComposedMessage msg)
        {
            Message = msg;
            Index = 0; 
        }
    }
}
