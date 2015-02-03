using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using Server.InterCom;
using Server2.NewInterCom.Com.Client; 


namespace Server.NewInterCom.Com
{
    public delegate void PacketHandler(InternalNetworkCommands Command, byte[] Packet,IPAddress Address,SendType type );

    public class ComLink
    {
        public IPEndPoint Address { get; private set; }

        private UdpClient _udpListener;
        private TcpListener _listener;
        private MulticastManager _multicast;

        private readonly DataCollector _udpCollector = new DataCollector();
        private readonly HashSet<IInternal> _tcpConnectedServers = new HashSet<IInternal>();

        private readonly List<IPEndPoint> _acceptedClients = new List<IPEndPoint>(); 

        public ComLink(IPEndPoint address)
        {
            if (PortIsUsed(address.Port))
                throw new InvalidOperationException("Port is Used");

            Address = address;
            
            SetupNetwork();
            _udpCollector.OnPacketRecived += UDPCollector_OnPacketRecived;
            _multicast.OnMulticastRecived += Multicast_OnMulticastRecived;
        }

        private void SetupNetwork()
        {
            _multicast = new MulticastManager();

            _listener = new TcpListener(Address.Address,Address.Port);
            _listener.BeginAcceptTcpClient(TCPConnection_Request, this);

            _udpListener = new UdpClient(Address.Port);
            _udpListener.BeginReceive(UDPData_Recived, null);

        }

        private void UDPData_Recived(IAsyncResult ar)
        {
            try
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any,0);
                var buffer = _udpListener.EndReceive(ar, ref endpoint);
                _udpCollector.AddData(buffer, endpoint.Address);
                _udpListener.BeginReceive(UDPData_Recived, null);
            }
            catch
            {
                
            }
        }

        private void TCPConnection_Request(IAsyncResult ar)
        {
            TcpClient client = null; 
            try
            {
                _listener.BeginAcceptTcpClient(TCPConnection_Request, null);
                client = _listener.EndAcceptTcpClient(ar);
                if (true)
                {
                    InternalClient remoteServer = new InternalClient(client);
                    remoteServer.OnDataRecived += remoteServer_OnDataRecived;
                    remoteServer.OnDisconnect += remoteServer_Disconnected;
                    lock (_tcpConnectedServers)
                    {
                        _tcpConnectedServers.Add(remoteServer);
                    }
                }
                else
                {
                    client.Close();
                }
            }
            catch
            {
                if (client != null)
                    client.Close();
            }

        }

        private void remoteServer_Disconnected(IInternal obj)
        {
            obj.OnDataRecived -= remoteServer_OnDataRecived;
            obj.OnDisconnect -= remoteServer_Disconnected;
            lock (_tcpConnectedServers)
            {
                _tcpConnectedServers.Remove(obj);
            }
        }

        #region Data Recived

        public event PacketHandler PacketRecived; 

        private void FirePacketRecived(InternalNetworkCommands command, byte[] packet,IPAddress address,SendType type )
        {
            PacketHandler Event = PacketRecived;
            if (Event != null)
                Task.Factory.StartNew(() => Event(command, packet, address, type));
        }

        private void remoteServer_OnDataRecived(InternalNetworkCommands command, byte[] packet, IInternal item)
        {
            FirePacketRecived(command, packet, item.IP, SendType.TCP);
        }

        private void UDPCollector_OnPacketRecived(IPAddress address, InternalNetworkCommands command, byte[] packet)
        {
            FirePacketRecived(command, packet, address, SendType.UDP);
        }

        private void Multicast_OnMulticastRecived(byte[] packet, IPEndPoint address)
        {
            InternalNetworkCommands command = (InternalNetworkCommands)packet[0]; 
            byte[] data = new byte[packet.Length-1];
            Array.Copy(packet, 1, data, 0, data.Length);
            FirePacketRecived(command, data, IPAddress.Broadcast, SendType.Multicast);
        }

        #endregion

        #region Send
        public void Send(InternalNetworkCommands command, byte[] packet, IPAddress ipAddress, SendType type = SendType.TCP)
        {
            if (type == SendType.Multicast)
            {
                SendMulticast(command, packet);
            }
            else
            {
                IPEndPoint address;
                lock(_acceptedClients)
                    address = _acceptedClients.FirstOrDefault((o) => o.Address.Equals(ipAddress));
                if (address != null)
                {
                    switch (type)
                    {
                        case SendType.TCP:
                            SendTcp(command, packet, address);
                            break;
                        case SendType.UDP:
                            SendUDP(command, packet, address);
                            break;
                    }
                }
            }
        }

        private void SendTcp(InternalNetworkCommands command, byte[] packet, IPEndPoint address)
        {
            try
            {
                if (true)
                {
                    lock (_tcpConnectedServers)
                    {
                        IInternal client = _tcpConnectedServers.FirstOrDefault((o) => o.IP.Equals(address));
                        if (client == null)
                        {
                            TcpClient tcpclient = new TcpClient();
                            var item = new ConnectionRQ(tcpclient, command, packet, address.Address);
                            tcpclient.BeginConnect(address.Address, address.Port, Connection_Done, item);
                            _tcpConnectedServers.Add(item);
                        }
                        else
                        {
                            client.Send(command, packet);
                        }
                    }
                }
            }
            catch
            {
            }

        }

        private void Connection_Done(IAsyncResult ar)
        {
            ConnectionRQ clinet = ar.AsyncState as ConnectionRQ;
            try
            {
                clinet.tcpclient.EndConnect(ar);
                InternalClient remoteServer = new InternalClient(clinet.tcpclient);
                remoteServer.OnDataRecived += remoteServer_OnDataRecived;
                remoteServer.OnDisconnect += remoteServer_Disconnected;
                lock (_tcpConnectedServers)
                {
                    _tcpConnectedServers.Remove(clinet);
                    _tcpConnectedServers.Add(remoteServer);
                }
                for (int i = 0; i < clinet.Data.Count; i++)
                {
                    remoteServer.Send(clinet.Data[i].Command, clinet.Data[i].Data);
                }
            }
            catch
            {
                lock (_tcpConnectedServers)
                {
                    _tcpConnectedServers.Remove(clinet);
                }
            }
        }

        private void SendUDP(InternalNetworkCommands command, byte[] packet, IPEndPoint address)
        {
            byte[] data = new byte[packet.Length+2]; 
            data[0] = (byte)command; 
            data[1] = (byte)(packet.Length);
            Array.Copy(packet,0,data,3,packet.Length);

            _udpListener.BeginSend(data, data.Length,address, PacketSends_Data, null); 
        }

        private void PacketSends_Data(IAsyncResult ar)
        {
            try
            {
                _udpListener.EndSend(ar); 
            }
            catch
            {

            }
        }

        private void SendMulticast(InternalNetworkCommands command, byte[] packet)
        {
            byte[] data = new byte[packet.Length + 1];
            data[0] = (byte)command;
            Array.Copy(packet, 0, data, 1, packet.Length);
            _multicast.Send(data); 
        }

        #endregion

        public bool IsAccepted(IPAddress address)
        {
            lock (_acceptedClients)
            {
                return null != _acceptedClients.FirstOrDefault((o) => o.Address.Equals(address)); 
            }
        }

        public void AddAcceptedAddress(IPAddress address, int port)
        {
            lock (_acceptedClients)
            {
                _acceptedClients.Add(new IPEndPoint(address, port));    
            }
        }

        private bool PortIsUsed(int port)
        {
            bool alreadyinuseUDP = (from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners() where p.Port == port select p).Count() == 1;
            bool alreadyinuseTCP = (from p in System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners() where p.Port == port select p).Count() == 1;

            return alreadyinuseUDP || alreadyinuseUDP; 
        }

    }

    public enum SendType
    {
        TCP, 
        UDP, 
        Multicast
    }

}
