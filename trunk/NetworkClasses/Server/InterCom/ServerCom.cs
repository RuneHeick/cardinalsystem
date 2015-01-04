using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.InterCom
{
    public class ServerCom
    {
        private MulticastManager Multicast = new MulticastManager();
        private Dictionary<string, AddressInfo> Addresses = new Dictionary<string, AddressInfo>();

        private readonly object NetStateLock = new object(); 
        private ushort NetState = 0;

        private IPEndPoint Me;


        public ServerCom()
        {
            Multicast.OnMulticastRecived += Multicast_OnMulticastRecived;
            
            
        }


        void Multicast_OnMulticastRecived(byte[] data, IPEndPoint From)
        {
             if(data[0] == (byte)InterComCommands.IAm)
             {
                 IAmCom com = new IAmCom();
                 com.Command = data;
                 AddOrUpdateAddress(com.IP, com.Port, com.NetState);

                 if (NetState != com.NetState)
                     SendWho(); 

             }
             else if(data[0] == (byte)InterComCommands.Who)
             {
                 WhoCom com = new WhoCom();
                 com.Command = data;
                 AddOrUpdateAddress(com.IP, com.Port, com.NetState);
                 SendIAm(); 
             }
        }

        private void SendWho()
        {
            WhoCom infoCollector = new WhoCom()
            {
                IP = Me.Address.ToString(),
                Port = Me.Port,
                NetState = NetState
            };

            Multicast.Send(infoCollector.Command); 
        }


        public void Start(IPEndPoint Address)
        {
            Me = Address;
            AddOrUpdateAddress(Me.Address.ToString(), Me.Port, NetState);
            SendWho();
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
           IAmCom me = new IAmCom()
           {
             IP = Me.Address.ToString(),
             Port = Me.Port,
             NetState = NetState
           };

           Multicast.Send(me.Command); 
               
        }

       

        private void AddOrUpdateAddress(string Ip, int port, ushort netState)
        {
            if (Addresses.ContainsKey(Ip))
            {
                lock (Addresses)
                {
                    var item = Addresses[Ip];
                    item.Address.Address = IPAddress.Parse(Ip);
                    item.Address.Port = port;
                    item.NetView = netState;
                }
            }
            else
            {

                AddressInfo info = new AddressInfo()
                    {
                        Address = new IPEndPoint(IPAddress.Parse(Ip), port),
                        NetView = netState
                    };
                lock (Addresses)
                {
                    Addresses.Add(Ip, info);
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

    }
}
