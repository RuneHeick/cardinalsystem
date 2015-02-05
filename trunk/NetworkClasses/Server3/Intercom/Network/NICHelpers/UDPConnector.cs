using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Network.NICHelpers
{
    public class UDPConnector : IConnector
    {

        IPEndPoint Me { get; private set; }
        Socket _Listener; 

        public UDPConnector(IPEndPoint address)
        {
            Me = address;
            _Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _Listener.Bind(address);
            _Listener.
        }


        private void AsyncUdpReciveComplete(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0,0);
                byte[] packet = _Listener.EndReceive(ar, ref from);
                _Listener.BeginReceive(AsyncUdpReciveComplete, null);
                if(packet.Length>2)
                {
                    
                }              

            }
            catch
            {

            }
        }



    }
}
