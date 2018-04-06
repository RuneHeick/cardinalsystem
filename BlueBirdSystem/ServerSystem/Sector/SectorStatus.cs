using System;
using System.Collections.Generic;
using System.Text;

namespace Sector
{

    public delegate void SectorStatusChanged(ISector sector, SectorStatus status);
    
    public enum SectorStatus
    {
        STOPPED,
        RUNNING_BUT_HAS_NO_WORK,
        RUNNING,
        STRESSED,
        ERROR
    }
}
