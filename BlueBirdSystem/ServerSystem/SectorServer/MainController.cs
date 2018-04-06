using Networking;
using NetworkingLayer;
using NetworkingLayer.Util;
using SectorController;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SectorServer
{
    class MainController
    {
        private TCPServer _tcpServer;
        private UDPServerClient _udpServer;

        private SectorCoordinator sectorController = new SectorCoordinator();


        public MainController(int tcpPort, int udpPort)
        {
            _tcpServer = new TCPServer(new IPEndPoint(IPAddress.Any, tcpPort));
            _tcpServer.OnSocketEvent += sectorController.Network_OnSocketEvent;

            _udpServer = new UDPServerClient(new IPEndPoint(IPAddress.Any, udpPort));

            sectorController.OnSendPacketOnNetwork += SectorController_OnSendPacketOnNetwork;

        }

        private bool SectorController_OnSendPacketOnNetwork(Networking.Util.ComposedMessage msg, string address, bool TryConnect)
        {

            return true;
        }

        public void Tick()
        {
            sectorController.Tick();

        }

    }
}
