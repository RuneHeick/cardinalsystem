using Sector;
using System;
using System.Collections.Generic;
using System.Text;

namespace SectorController.Util
{
    public class SectorsStatistics
    {
        public Dictionary<string, int> Statistic { get; set; } = new Dictionary<string, int>();
        public int TotalSectors { get; set; } = 0;

        public SectorsStatistics()
        {
            foreach(string name in Enum.GetNames(typeof(SectorStatus)))
            {
                Statistic.Add(name, 0);
            }
        }

        public void Incement(SectorStatus status)
        {
            lock(Statistic)
                Statistic[status.ToString()]++;
        }

        public void Decrement(SectorStatus status)
        {
            lock (Statistic)
                Statistic[status.ToString()]--;
        }


    }
}
