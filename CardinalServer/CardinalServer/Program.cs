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
            rune.Add(a);

            rune.Add(new TestItem());
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            Console.WriteLine("pre");
            Thread.Sleep(20000); 
            a = null;

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            Console.WriteLine("pre2");

            Console.WriteLine("Done");
            

        }
    }

    class TestItem: ManagedData
    {

        public override byte[] Serilize()
        {
            throw new NotImplementedException();
        }

        public override void Deserize(byte[] data)
        {
             throw new NotImplementedException();
        }

        
        
    }
}
