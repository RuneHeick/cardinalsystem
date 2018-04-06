using Sector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SectorController
{
    static class DiskCache
    {
        private const string CacheLocalPath = "Temp/Cache";
        private const string Ext = ".sec";

        static DiskCache()
        {
            if (Directory.Exists(CacheLocalPath))
                Directory.Delete(CacheLocalPath, true);
            Directory.CreateDirectory(CacheLocalPath);
        }


        public static async Task<SectorContainer> Load(ulong address)
        {
            //Just for test
            return new SectorContainer(new TestSector(address));
        }


        public static async Task Save(SectorContainer sector)
        {
            using (FileStream writer = new FileStream(CacheLocalPath + "/" + sector.Sector.Address + Ext,FileMode.Create, FileAccess.Write))
            {
                await writer.WriteAsync(new byte[] { (byte)sector.Status }, 0, 1);
                byte[] sectorData = sector.Sector.Save();
                await writer.WriteAsync(sectorData, 0, sectorData.Length);
            }
        }

    }

}

