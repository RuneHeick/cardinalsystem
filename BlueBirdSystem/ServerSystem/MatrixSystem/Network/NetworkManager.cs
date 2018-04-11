using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.Network
{
    public delegate void NetworkPacketHandler(ComposedMessageStream stream, VirtualSocket socket); 

    public class NetworkManager
    {

        public void NetworkManager_RecivedFromNetwork(byte[] message, VirtualSocket socket)
        {
            ComposedMessageStream stream = new ComposedMessageStream(message)
            {
                CurrentIndex = 1
            };

            switch ((BasePacketType)message[0])
            {
                case BasePacketType.NETWORK_UPDATE:
                    OnUpdatePackets?.Invoke(stream, socket);
                break;
                case BasePacketType.NETWORK_COMMAND:
                    OnCommandPackets?.Invoke(stream, socket);
                break;
                case BasePacketType.NETWORK_INFO:
                    OnInfoPackets?.Invoke(stream, socket);
                break;
            }
        }

        public event NetworkPacketHandler OnUpdatePackets;

        public event NetworkPacketHandler OnCommandPackets;

        public event NetworkPacketHandler OnInfoPackets;

    }
}
