using System;

namespace NetworkingLayer.Util
{
    public class SocketEvent: EventArgs
    {
        public string Address { get; set; }

        public ConnectionState State { get; set; }

        public byte[] Data { get; set; }

    }
}
