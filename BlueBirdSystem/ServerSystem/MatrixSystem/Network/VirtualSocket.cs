using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.Network
{
    public class VirtualSocket
    {
        public static uint BytesGennerated;

        internal void Send(ComposedMessage packet)
        {
            BytesGennerated += (uint)packet.Length;
        }
    }
}
