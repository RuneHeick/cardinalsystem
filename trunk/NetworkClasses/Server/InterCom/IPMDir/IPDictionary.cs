using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom.IPMDir
{
    public class IPDictionary
    {
        private Dictionary<IPAddress, AddressInfo> Addresses = new Dictionary<IPAddress, AddressInfo>(new IPEqualityComparer());
        AddressInfo Me = null;

        public IPDictionary(IPEndPoint me)
        {
            Update(me, 0);
            Me = Addresses.Values.ElementAt(0); 
        }
        
        public ushort NetState
        {
            get
            {
                if(Me != null)
                    return Me.NetView;
                return 0; 
            }
            private set
            {
                if (Me != null)
                {
                    if (Me.NetView != value)
                    {
                        Me.NetView = value;
                    }
                }
            }
        }

        public bool IsSync
        {
            get
            {
                return isSync_;
            }
        }
        private bool isSync_;
        public event Action<bool> IsSyncChanged;

        public IPEndPoint Master
        {
            get
            {
                return master_; 
            }
            private set
            {
                if (!master_.Address.Equals(value.Address))
                {
                    master_ = value;
                    Action<IPEndPoint> Event = MasterChanged;
                    if (Event != null)
                    {
                        Task.Factory.StartNew(() => Event(master_));
                    }
                }
            }
        }
        private IPEndPoint master_ = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
        public event Action<IPEndPoint> MasterChanged;

        public void Update(IPEndPoint Ip, ushort netState)
        {
            var ip = Ip.Address;

            if(Addresses.ContainsKey(ip))
            {
                var item = Addresses[ip]; 
                if(item.NetView != netState)
                {
                    lock (Addresses)
                    {
                        item.NetView = netState;
                    }
                    NetState = CalcNetState();
                    UpdateIsSync();
                }
            }
            else
            {
                lock (Addresses)
                {
                    Addresses.Add(ip, new AddressInfo() { Address = Ip, NetView = netState });
                }
                if (IPAddressToLong(ip) > IPAddressToLong(Master.Address))
                {
                    Master = Ip;
                }
                NetState = CalcNetState();
                UpdateIsSync();
                Console.WriteLine(" added " + ip.ToString());
            }
        }

        public void Remove(IPAddress ip)
        {
            if (Addresses.ContainsKey(ip))
            {
                lock (Addresses)
                {
                    Addresses.Remove(ip);
                }
                NetState = CalcNetState();
                UpdateIsSync();
                if (ip.Equals(Master.Address))
                    FindNewMaster();
            }
        }

        private void FindNewMaster()
        {
            IPEndPoint master =  Addresses.Values.ElementAt(0).Address; 
            lock(Addresses)
            {
                foreach(AddressInfo info in Addresses.Values)
                {
                    if(IPAddressToLong(master.Address)<IPAddressToLong(info.Address.Address))
                    {
                        master = info.Address; 
                    }
                }
            }
            Master = master; 
        }

        private ushort CalcNetState()
        {
            ushort id = 0;
            lock (Addresses)
            {
                List<string> IPs = Addresses.Keys.ToList().ConvertAll( (o)=> o.ToString() );
                IPs.Sort();
                Crc16 CRC = new Crc16();
                foreach (string ip in IPs)
                {
                    id = CRC.addBytes(IPAddress.Parse(ip).GetAddressBytes());
                }
            }

            return id;
        }

        private void UpdateIsSync()
        {
            bool state;
            lock (Addresses)
            {
                state = Addresses.Values.All((o) => o.NetView == NetState);
            }
            if (state != isSync_)
            {
                isSync_ = state;
                Action<bool> Event = IsSyncChanged;
                if (Event != null)
                {
                    Task.Factory.StartNew(() => Event(state));
                }
            }
        }

        static public uint IPAddressToLong(IPAddress IPAddr)
        {
            if (IPAddr == null) return 0; 

            byte[] byteIP = IPAddr.GetAddressBytes();

            uint ip = (uint)byteIP[3] << 24;
            ip += (uint)byteIP[2] << 16;
            ip += (uint)byteIP[1] << 8;
            ip += (uint)byteIP[0];

            return ip;
        }

        public bool Contains(IPAddress ip)
        {
            return Addresses.ContainsKey(ip); 
        }

        public IPEndPoint this[IPAddress ip]
        {
            get
            {
                if (Addresses.ContainsKey(ip))
                {
                    return Addresses[ip].Address; 
                }
                return null; 
            }
        }

        public List<IPAddress> IPs
        {
            get
            {
                return Addresses.Keys.ToList(); 
            }
        }

    }

    class IPEqualityComparer : IEqualityComparer<IPAddress>
    {
        public bool Equals(IPAddress b1, IPAddress b2)
        {
            return b1.Equals(b2); 
        }


        public int GetHashCode(IPAddress bx)
        {
           return bx.GetHashCode();
        }
    }



}
