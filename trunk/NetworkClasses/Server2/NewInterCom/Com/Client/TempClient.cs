using Server.InterCom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.NewInterCom.Com
{
    class ConnectionRQ : IInternal
    {
        public ConnectionRQ(TcpClient tcpclient1, InternalNetworkCommands command, byte[] Data, IPAddress ip)
        {
            this.tcpclient = tcpclient1;
            this.Data = new List<CollectionItem>(2);
            this.Data.Add(new CollectionItem() {Data = Data, Command = command });
            this.IP = ip;
        }

        public TcpClient tcpclient { get; set; }
        public IPAddress IP { get; private set; }

        public List<CollectionItem> Data { get; set; }

        public void Send(InternalNetworkCommands commands, byte[] data)
        {
            Data.Add(new CollectionItem() { Data = data, Command = commands });
        }

        public void FireOnDisconnect()
        {
            Action<IInternal> myEvent = OnDisconnect;
            if (myEvent != null)
                myEvent(this);
        }

        public struct CollectionItem
        {
            public byte[] Data { get; set; }
            public InternalNetworkCommands Command { get; set; }
        }

        public event Action<IInternal> OnDisconnect;

        public event Action<InternalNetworkCommands, byte[], IInternal> OnDataRecived;
    }
}
