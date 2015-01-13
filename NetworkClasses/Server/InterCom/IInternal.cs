using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    interface IInternal
    {

        void Send(InternalNetworkCommands commands, byte[] data);
        IPAddress IP { get;}


        event Action<IInternal> OnDisconnect;
        event Action<InternalNetworkCommands, byte[], IInternal> OnDataRecived;

    }
}