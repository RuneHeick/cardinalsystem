using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModules.Connection.Helpers
{
    class Connection
    {
        internal static void AddSocket(System.Net.Sockets.TcpClient socket)
        {
            throw new NotImplementedException();
        }

        internal static Connection GetConnection(IPEndPoint Address, int maxIdleTime)
        {
            throw new NotImplementedException();
        }

        public ConnectionStatus Status { get; set; }
    }


    public enum ConnectionStatus
    {
        Connected,
        Connecting,
        Disconnected
    }



}
