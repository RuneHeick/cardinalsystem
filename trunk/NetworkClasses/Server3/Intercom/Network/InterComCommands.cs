﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom
{
    enum InterComCommands
    {
        PortMessage = 0,
        PacketInfo,
        PacketRecive,
        PeriodicMsg,
        Extend = 31
    }
}
