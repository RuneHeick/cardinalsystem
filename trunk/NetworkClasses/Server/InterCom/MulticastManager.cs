using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MulticastManager
    {
        private UdpClient MulticastClient { get; set; }
        private static IPEndPoint MulticastAddress = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 2222);
        
        public MulticastManager()
        {
            MulticastClient = new UdpClient();
            MulticastClient.JoinMulticastGroup(MulticastAddress.Address);
            MulticastClient.MulticastLoopback = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, MulticastAddress.Port);
            MulticastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            MulticastClient.Client.Bind(localEp);
            MulticastClient.BeginReceive(MulticastRecived, MulticastClient); 
        }

        private void MulticastRecived(IAsyncResult ar)
        {
            try
            {
                IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = MulticastClient.EndReceive(ar, ref end);
                if (data.Length != 0)
                {
                    if (OnMulticastRecived != null)
                    {
                        OnMulticastRecived(data, MulticastAddress);
                    }
                }
                else
                {
                    multicastError(); 
                }
            }
            catch
            {
                multicastError(); 
            }
            
        }

        private void multicastError()
        {
            throw new NotImplementedException();
        }

        public event Action<byte[], IPEndPoint > OnMulticastRecived; 

        public void Send(byte[] data)
        {
            try
            {
                MulticastClient.BeginSend(data, data.Length,MulticastAddress,SendComplete, MulticastClient);
            }
            catch
            {
                multicastError(); 
            }
        }

        private void SendComplete(IAsyncResult ar)
        {
            try
            {
                MulticastClient.EndSend(ar);
            }
            catch
            {
                 multicastError(); 
            }
        }




    }
}
