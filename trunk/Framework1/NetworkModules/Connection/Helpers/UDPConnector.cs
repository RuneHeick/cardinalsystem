using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkModules.Connection.Helpers
{
    public class UDPConnector
    {

        readonly UdpClient _listener;

        public UDPConnector(IPEndPoint endPoint)
        {
            _listener = new UdpClient(endPoint);
        }

        private void StartRecive()
        {
            _listener.BeginReceive(AsyncUdpReciveComplete, null);
        }

        private void AsyncUdpReciveComplete(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] packetBytes = _listener.EndReceive(ar, ref from);
                StartRecive();

                if (0 != packetBytes.Length)
                {
                    PacketsRecived(packetBytes, from);
                }

            }
            catch (ObjectDisposedException e)
            { }
            catch (Exception e)
            {
                StartRecive();
            }
        }

        private void PacketsRecived(byte[] packetBytes, IPEndPoint from)
        {
            Connection con = Connection.GetConnection(from, Timeout.Infinite);
        }



    }
}
