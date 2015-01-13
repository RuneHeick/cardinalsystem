using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    public enum InternalNetworkCommands
    {
        Login = 0,
        Data = 1,
        AcceptedLogin = 3,
        Signal = 4, 
        SignalResponse = 5
    }


}
