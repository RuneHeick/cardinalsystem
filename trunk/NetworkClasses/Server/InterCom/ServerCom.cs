using Server.Client;
using Server.InterCom.IPMDir;
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
        private IPDictionary Addresses; 


        private HashSet<IInternal> ConnectedServers = new HashSet<IInternal>();

        private IPEndPoint Me;
        

        private TcpListener AcceptSocketInternal;
        private Task SendIamTask; 


        public ServerCom(IPEndPoint Address)
        {
            Me = Address;
            Addresses = new IPDictionary(Me); 
            AcceptSocketInternal = new TcpListener(Address.Address,0);
            AcceptSocketInternal.Start();
            Me.Port = (AcceptSocketInternal.Server.LocalEndPoint as IPEndPoint).Port;
            AcceptSocketInternal.BeginAcceptTcpClient(Connect_Request,null);
            Multicast.OnMulticastRecived += Multicast_OnMulticastRecived;

            Addresses.IsSyncChanged += Addresses_IsSyncChanged;
            Addresses.MasterChanged += Addresses_MasterChanged;
            SendWho();
            
        }

        void Addresses_MasterChanged(IPEndPoint obj)
        {
            Console.WriteLine("Master is: " + obj.Address.ToString());
        }

        

        public bool Send(IPAddress ip, byte[] Data)
        {
            bool ret = true; 
            try
            {
                if (Addresses.Contains(ip))
                {
                    lock (ConnectedServers)
                    {
                        IInternal client = ConnectedServers.FirstOrDefault((o) => o.IP.Equals(ip));
                        if (client == null)
                        {
                            var info = Addresses[ip];
                            TcpClient tcpclient = new TcpClient();
                            var item = new ConnectionRQ(tcpclient, Data, info.Address);
                            tcpclient.BeginConnect(info.Address, info.Port, Connection_Done, item);
                            ConnectedServers.Add(item);
                        }
                        else
                        {
                            client.Send(InternalNetworkCommands.Data, Data);
                        }
                    }
                }
                else
                    ret = false; 
            }
            catch
            { ret = false; }
            return ret; 
        }

       
        private void Connect_Request(IAsyncResult ar)
        {
            TcpClient client = null; 
            try
            {
                AcceptSocketInternal.BeginAcceptTcpClient(Connect_Request, null);
                client = AcceptSocketInternal.EndAcceptTcpClient(ar);
                if(Addresses.Contains((client.Client.RemoteEndPoint as IPEndPoint).Address))
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

        private void remoteServer_Disconnected(IInternal obj)
        {
            obj.OnDataRecived -= remoteServer_OnDataRecived;
            obj.OnDisconnect -= remoteServer_Disconnected;
            lock (ConnectedServers)
            {
                ConnectedServers.Remove(obj);
            }
        }

        void remoteServer_OnDataRecived(InternalNetworkCommands arg1, byte[] arg2, IInternal arg3)
        {
            Console.WriteLine(BitConverter.ToString(arg2));
        }

        private class ConnectionRQ : IInternal
        {
            public ConnectionRQ(TcpClient tcpclient1, byte[] Data, IPAddress ip)
            {
                this.tcpclient = tcpclient1;
                this.Data = new List<byte[]>(1); 
                this.Data.Add(Data);
                this.IP = ip;
            }
            public TcpClient tcpclient { get; set; }
            public IPAddress IP { get; private set; }

            public List<byte[]> Data { get; set; }

            public void Send(InternalNetworkCommands commands, byte[] data)
            {
                Data.Add(data); 
            }

            public void FireOnDisconnect()
            {
                Action<IInternal> myEvent = OnDisconnect;
                            if (myEvent != null)
                                myEvent(this);
            }

            public event Action<IInternal> OnDisconnect;

            public event Action<InternalNetworkCommands, byte[], IInternal> OnDataRecived;
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
                    ConnectedServers.Remove(clinet);
                    ConnectedServers.Add(remoteServer);
                }
                for (int i = 0; i < clinet.Data.Count; i++ )
                {
                    remoteServer.Send(InternalNetworkCommands.Data, clinet.Data[i]); 
                }
            }
            catch
            {
                lock (ConnectedServers)
                {
                    ConnectedServers.Remove(clinet);
                }
                Addresses.Remove(clinet.IP); 
            }
        }
        private void Multicast_OnMulticastRecived(byte[] data, IPEndPoint From)
        {
            if (data[0] == (byte)InterComCommands.IAm)
            {
                IAmCom com = new IAmCom();
                com.Command = data;
                Addresses.Update(new IPEndPoint(IPAddress.Parse(com.IP), com.Port), com.NetState);

            }
            else if (data[0] == (byte)InterComCommands.Who)
            {
                WhoCom com = new WhoCom();
                com.Command = data;
                Addresses.Update(new IPEndPoint(IPAddress.Parse(com.IP), com.Port), com.NetState);
                SendIAm();
            }
            else if (data[0] == (byte)InterComCommands.Offline)
            {
                OfflineCom off = new OfflineCom() { Command = data };
                Addresses.Remove(IPAddress.Parse(off.IP));
            }
        }

        void Addresses_IsSyncChanged(bool obj)
        {
            if (!obj)
            {
                SendIAm();
                if (SendIamTask == null || SendIamTask.Status != TaskStatus.Running)
                    SendIamTask = Task.Factory.StartNew(InfoTask);
                Console.WriteLine("Not Sync");
            }
            else
                Console.WriteLine("Sync");
        }

        private void SendIAm()
        {
            IAmCom infoCollector = new IAmCom()
            {
                IP = Me.Address.ToString(),
                Port = Me.Port,
                NetState = Addresses.NetState
            };

            Multicast.Send(infoCollector.Command);
        }

        private async void SendWho()
        {
            WhoCom infoCollector = new WhoCom()
            {
                IP = Me.Address.ToString(),
                Port = Me.Port,
            };
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(100);
                infoCollector.NetState = Addresses.NetState;
                Multicast.Send(infoCollector.Command);
            }
        }

        private async void InfoTask()
        {
            bool running = true;
            while (running)
            {
                WhoCom me = new WhoCom()
                {
                    IP = Me.Address.ToString(),
                    Port = Me.Port,
                    NetState = Addresses.NetState
                };

                Multicast.Send(me.Command); 

                lock (Addresses)
                {
                    if (Addresses.IsSync)
                        break;
                }

                await Task.Delay(2000);
            }
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
