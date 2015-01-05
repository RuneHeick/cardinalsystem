using Server.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.InterCom
{
    public class ServerCom
    {
        private MulticastManager Multicast = new MulticastManager();
        private Dictionary<string, AddressInfo> Addresses = new Dictionary<string, AddressInfo>();

        private HashSet<InternalClient> ConnectedServers = new HashSet<InternalClient>();
        private readonly object NetStateLock = new object(); 
        private ushort NetState = 0;

        private IPEndPoint Me;
        private Task SendIamTask;

        private TcpListener AcceptSocketInternal; 


        public ServerCom(IPEndPoint Address)
        {
            Me = Address;
            
            AcceptSocketInternal = new TcpListener(Address.Address,0);
            AcceptSocketInternal.Start();
            Me.Port = (AcceptSocketInternal.Server.LocalEndPoint as IPEndPoint).Port;
            AcceptSocketInternal.BeginAcceptTcpClient(Connect_Request,null);
            Multicast.OnMulticastRecived += Multicast_OnMulticastRecived;
            
            AddOrUpdateAddress(Me.Address.ToString(), Me.Port, NetState);
            SendWho();
            
        }

        private void Connect_Request(IAsyncResult ar)
        {
            TcpClient client = null; 
            try
            {
                AcceptSocketInternal.BeginAcceptTcpClient(Connect_Request, null);
                client = AcceptSocketInternal.EndAcceptTcpClient(ar);
                if(Addresses.ContainsKey((client.Client.RemoteEndPoint as IPEndPoint).Address.ToString()))
                {
                    InternalClient remoteServer = new InternalClient(client);
                    remoteServer.OnDataRecived += remoteServer_OnDataRecived;
                    remoteServer.OnDisconnect += remoteServer_Disconnected;
                    lock(ConnectedServers)
                    {
                        ConnectedServers.Add(remoteServer);
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

        private void remoteServer_Disconnected(InternalClient obj)
        {
            obj.OnDataRecived -= remoteServer_OnDataRecived;
            obj.OnDisconnect -= remoteServer_Disconnected;
            lock (ConnectedServers)
            {
                ConnectedServers.Remove(obj);
            }
        }

        void remoteServer_OnDataRecived(InternalNetworkCommands arg1, byte[] arg2, InternalClient arg3)
        {
            Console.WriteLine(BitConverter.ToString(arg2));
        }

        public void Send(IPAddress Address, byte[] Data)
        {
            string ip = Address.ToString();
            if (Addresses.ContainsKey(ip))
            {
                var info = Addresses[ip];
                InternalClient client = ConnectedServers.FirstOrDefault((o) => o.IP == Address);
                if (client == null)
                {
                    TcpClient tcpclient = new TcpClient();
                    tcpclient.BeginConnect(info.Address.Address, info.Address.Port, Connection_Done, new ConnectionRQ(tcpclient, Data, ip));
                }
                else
                {
                    client.Send(InternalNetworkCommands.Data, Data);
                }
            }
        }

        private class ConnectionRQ
        {
            public ConnectionRQ(TcpClient tcpclient1, byte[] Data, string ip)
            {
                // TODO: Complete member initialization
                this.tcpclient = tcpclient1;
                this.Data = Data;
                this.ip = ip;
            }
            public TcpClient tcpclient { get; set; }
            public string ip { get; set; }

            public byte[] Data { get; set; }
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
                lock (ConnectedServers)
                {
                    ConnectedServers.Add(remoteServer);
                }
                remoteServer.Send(InternalNetworkCommands.Data, clinet.Data); 
            }
            catch
            {
                RemoveFromAddresses(clinet.ip); 
            }
        }
        void Multicast_OnMulticastRecived(byte[] data, IPEndPoint From)
        {
            if (data[0] == (byte)InterComCommands.IAm)
            {
                IAmCom com = new IAmCom();
                com.Command = data;
                AddOrUpdateAddress(com.IP, com.Port, com.NetState);

                if (NetState != com.NetState)
                    SendWho();

            }
            else if (data[0] == (byte)InterComCommands.Who)
            {
                WhoCom com = new WhoCom();
                com.Command = data;
                AddOrUpdateAddress(com.IP, com.Port, com.NetState);
                SendIAm();
            }
            else if (data[0] == (byte)InterComCommands.Offline)
            {
                OfflineCom off = new OfflineCom() { Command = data };
                RemoveFromAddresses(off.IP);
            }
        }

        private void RemoveFromAddresses(string ip)
        {

            if (Addresses.ContainsKey(ip))
            {
                lock (Addresses)
                {
                    Addresses.Remove(ip);
                }
                Console.WriteLine("Offline "+ip);
            }
        }

        private async void SendWho()
        {
            WhoCom infoCollector = new WhoCom()
            {
                IP = Me.Address.ToString(),
                Port = Me.Port,
                NetState = NetState
            };

            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(500); 
                Multicast.Send(infoCollector.Command);
            }
        }

        private ushort CalcNetState()
        {
            ushort id = 0;
            lock (Addresses)
            {
                List<string> IPs = Addresses.Keys.ToList();
                IPs.Sort();
                Crc16 CRC = new Crc16();
                foreach (string ip in IPs)
                {
                    id = CRC.addBytes(IPAddress.Parse(ip).GetAddressBytes());
                }
            }

            return id; 
        }

        private void SendIAm()
        {
            if (SendIamTask == null || SendIamTask.Status != TaskStatus.Running)
                SendIamTask = Task.Factory.StartNew(InfoTask); 
        }

        private async void InfoTask()
        {
            bool running = true;
            while (running)
            {
                IAmCom me = new IAmCom()
                {
                    IP = Me.Address.ToString(),
                    Port = Me.Port,
                    NetState = NetState
                };

                Multicast.Send(me.Command); 

                lock (Addresses)
                {
                    if (!Addresses.Values.All((o) => o.NetView == NetState))
                        break;
                }

                await Task.Delay(2000);
            }
        }

        private void AddOrUpdateAddress(string Ip, int port, ushort netState)
        {
            if (Addresses.ContainsKey(Ip))
            {
                lock (Addresses)
                {
                    var item = Addresses[Ip];
                    item.NetView = netState;
                }
            }
            else
            {

                AddressInfo info = new AddressInfo()
                    {
                        Address = new IPEndPoint(IPAddress.Parse(Ip), port),
                        NetView = netState,
                    };
                lock (Addresses)
                {
                    Addresses.Add(Ip, info);
                    Console.WriteLine("Adding " + Ip);
                }
                UpdateNetState();
            }
        }

        private void UpdateNetState()
        {
            lock(NetStateLock)
            {
                NetState = CalcNetState(); 
            }
        }

        public bool IsInternal(TcpClient client)
        {
            return Addresses.ContainsKey(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()); 
        }

        public void HandelInternalConnection(TcpClient client)
        {

        }

        //This is a Knuth hash
        static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        private class AddressInfo
        {
            public IPEndPoint Address { get; set; }

            public ushort NetView { get; set; }

        }

        

        ~ServerCom()
        {
            OfflineCom infoCollector = new OfflineCom()
            {
                IP = Me.Address.ToString(),
            };

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(500);
                Multicast.Send(infoCollector.Command);
            }
        }
    }
}
