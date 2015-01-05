using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    interface IInternalClient
    {

        void Send(InternalNetworkCommands commands, byte[] data);
        IPAddress IP { get;}


        event Action<IInternalClient> OnDisconnect;
        event Action<InternalNetworkCommands, byte[], IInternalClient> OnDataRecived;

    }
}