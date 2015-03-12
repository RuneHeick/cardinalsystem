using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NetworkModules.Connection.Packet;

namespace Intercom.Protocols.Elements
{
    public class ClusterElement : PacketElement
    {

        internal List<ClientInfo> Clients
        {
            get { return _clients; }
        }

        private List<ClientInfo> _clients = new List<ClientInfo>();

        public void Add(IPEndPoint endPoint, int preformansId)
        {
            lock (_clients)
            {
                var item = _clients.FirstOrDefault((o) => o.EndPoint.Equals(endPoint));
                if (item == null)
                {
                    _clients.Add(new ClientInfo(endPoint, preformansId));
                }
                else
                {
                    item.PerformanceIndex = preformansId;
                }
            }
        }

        public override byte[] Data
        {
            get
            {
                lock (_clients)
                {
                    var data = new byte[_clients.Count*12];
                    for (int i = 0; i < _clients.Count; i++)
                    {
                        byte[] address = _clients[i].EndPoint.Address.GetAddressBytes();
                        Array.Copy(address, 0, data, i*12, 4);
                        byte[] port = BitConverter.GetBytes(_clients[i].EndPoint.Port);
                        Array.Copy(port, 0, data, (i*12) + 4, 4);
                        byte[] preformansId = BitConverter.GetBytes(_clients[i].PerformanceIndex);
                        Array.Copy(preformansId, 0, data, (i*12) + 8, 4);
                    }

                    return data;
                }
            }
            set
            {
                lock (_clients)
                {
                    byte[] rawBytes = value;
                    _clients.Clear();
                    for (int i = 0; i < rawBytes.Length;)
                    {
                        byte[] addressBytes = new byte[4];
                        Array.Copy(rawBytes, i, addressBytes, 0, 4);
                        IPAddress address = new IPAddress(addressBytes);
                        i = i + 4;

                        int port = BitConverter.ToInt32(rawBytes, i);
                        i = i + 4;

                        int preformansId = BitConverter.ToInt32(rawBytes, i);
                        i = i + 4;
                        _clients.Add(new ClientInfo(address, port, preformansId));
                    }
                }
            }
        }

        public override Size ExpectedSize
        {
            get { return Size.Dynamic; }
        }

        internal class ClientInfo
        {

            public ClientInfo(IPEndPoint endPoint, int preformansId)
            {
                EndPoint = endPoint;
                PerformanceIndex = preformansId; 
            }

            public ClientInfo(IPAddress address, int port, int preformansId)
            {
                EndPoint = new IPEndPoint(address, port);
                PerformanceIndex = preformansId; 
            }

            public IPEndPoint EndPoint { get; set; }

            public int PerformanceIndex { get; set; }

        }

    }
}
