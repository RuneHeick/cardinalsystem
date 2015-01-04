using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface INetCom
    {
        Socket Socket {get;}

        void ReadRequest(); 



    }
}
