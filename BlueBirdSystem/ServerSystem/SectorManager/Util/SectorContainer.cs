using Sector;
using System;
using System.Collections.Generic;
using System.Text;

namespace SectorController
{
    class SectorContainer
    {
        public ISector Sector { get; set; }
        public SectorStatus Status { get; set; }

        public DateTime LastUsed { get; set; }

        public SectorContainer(ISector sector)
        {
            Sector = sector;
            Status = SectorStatus.STOPPED;
        }

    }
}
