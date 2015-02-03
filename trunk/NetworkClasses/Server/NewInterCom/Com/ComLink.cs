using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net; 


namespace Server.NewInterCom.Com
{
    class ComLink
    {

        public IPEndPoint Address { get; private set; }

        private UdpClient UDPListener;
        private TcpListener Listener;
        private MulticastManager Multicast;

        private DataCollector UDPCollector = new DataCollector();

        public ComLink(IPEndPoint address)
        {
            if (PortIsUsed(address.Port))
                throw new InvalidOperationException("Port is Used");

            Address = address;
            SetupNetwork(); 
        }

        private void SetupNetwork()
        {
            Multicast = new MulticastManager();

            Listener = new TcpListener(Address.Address,Address.Port);
            Listener.BeginAcceptTcpClient(TCPConnection_Request, this);

            UDPListener = new UdpClient(Address.Port);
            UDPListener.BeginReceive(UDPData_Recived, null);

        }

        private void UDPData_Recived(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any,0);
                var buffer = UDPListener.EndReceive(ar, ref endpoint);
                UDPCollector.AddData(buffer, endpoint.Address);
                UDPListener.BeginReceive(UDPData_Recived, null);
            }
            catch
            {
                
            }
        }

        private void TCPConnection_Request(IAsyncResult ar)
        {
            try
            {
                TcpClient client = Listener.EndAcceptTcpClient(ar);
                client.GetStream().
            }
            catch
            {

            }
        }

        private bool PortIsUsed(int port)
        {
            bool alreadyinuseUDP = (from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port == port select p).Count() == 1;
            bool alreadyinuseTCP = (from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners() where p.Port == port select p).Count() == 1;

            return alreadyinuseUDP || alreadyinuseUDP; 
        }

    }
}
