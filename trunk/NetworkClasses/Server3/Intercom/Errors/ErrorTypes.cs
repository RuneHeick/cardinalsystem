using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.Errors
{
    public enum ErrorType
    {
        TimeOut,
        Connection,
        RequestFull,
        PacketFormat
    }
}
