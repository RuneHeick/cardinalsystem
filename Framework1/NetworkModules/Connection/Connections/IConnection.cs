﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Connector;
using Server3.Intercom.Network.Packets;

namespace NetworkModules.Connection.Connections
{
    public interface IConnection
    {
        PacketType Supported { get; }

        IPEndPoint RemoteEndPoint { get; }

        void Send(NetworkPacket packet);

        void Close();

        event PacketHandler OnPacketRecived;
    }
}
