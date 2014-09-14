using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardinalTypes.Types;
using CardinalTypes.Data;
using System.Threading;

namespace CardinalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DataManager rune = new DataManager("test");
            TestItem a = new TestItem();
            ValueContainor t = rune.Add(a);
            rune.Add(new TestItem());
            Console.WriteLine("pre");
            a = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Thread.Sleep(10000);
            Console.WriteLine("pre2");

            Console.WriteLine("Done");
            

        }
    }

    class TestItem: IData
    {
        public byte[] Serilize()
        {
            throw new NotImplementedException();
        }

        public void Deserize(byte[] data)
        {
            throw new NotImplementedException();
        }

        public bool IsAlive
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
