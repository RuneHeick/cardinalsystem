using NetworkingLayer.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Networking
{

    public delegate void SocketEventHandler(INetwork sender, SocketEvent arr);

    public interface INetwork
    {
        event SocketEventHandler OnSocketEvent;
    }
}
