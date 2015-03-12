using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace Intercom
{
    public static class SystemTester
    {

        public static int GetStatus()
        {
            int ramid = 0;
            int processor = 0; 

            ObjectQuery wql = new ObjectQuery("SELECT * FROM Win32_Processor");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wql);
            ManagementObjectCollection results = searcher.Get();

            foreach (ManagementObject result in results)
            {
                processor += Convert.ToInt32(result["NumberOfLogicalProcessors"]) * Convert.ToInt32(result["MaxClockSpeed"]);
            }

            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_PhysicalMemory");
            searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject queryObj in searcher.Get())
            {
                ramid += Convert.ToInt32((Convert.ToInt64(queryObj["Capacity"]) / 100000000) + (Convert.ToInt64(queryObj["Speed"]) / 10));
            }


            return ramid + processor; 
        }



    }
}
