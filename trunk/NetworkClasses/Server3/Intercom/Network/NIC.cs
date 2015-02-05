using System.Net;
using Server3.Intercom.Network.NICHelpers;

namespace Server3.Intercom.Network
{
    class NIC
    {

        //Public 
        public IPEndPoint Ip { get; set; }


        //Privates 
        private TCPConnector _tcpConnector { get; set; }
        private UdpConnector _udpConnector { get; set; }
        private MulticastConnector _multicastConnector { get; set; }


        public NIC(IPEndPoint ip)
        {
            this.Ip = ip;
        }

        

    }
}
