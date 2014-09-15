using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    class FileNumberNamer
    {
        List<long> FreeNumbers = new List<long>();

        long StartNumber = 1;

        public FileNumberNamer(string Path)
        {


        }

        public long GetNext()
        {
            long nr = StartNumber;

            if(FreeNumbers.Count>0)
            {
                lock (FreeNumbers)
                {
                    nr = FreeNumbers[0];
                    FreeNumbers.RemoveAt(0);
                }
            }
            else
                StartNumber++;

            return nr; 
        }

        public void FreeNumber(long number)
        {
            lock (FreeNumbers)
                FreeNumbers.Add(number);
        }

    }
}
