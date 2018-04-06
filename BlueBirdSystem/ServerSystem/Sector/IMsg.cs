using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sector
{

    public delegate void SendMsgHandler(ISector sector, IMsg msg);

    public interface IMsg
    {
        ulong ToSector { get; }

        ComposedMessage GetMessage();
    }

    public interface ISectorMsg : IMsg
    {
        ulong FromSector { get; }
    }

    public interface IClientMsg: IMsg
    {
        string Client { get; }
    }
}
