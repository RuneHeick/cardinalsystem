﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardinalTypes.Types;
using CardinalTypes.Data;
using System.Threading;
using FileManager;
using ConfigurationManager;

namespace CardinalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationController rune = ConfigurationController.Instance;
            {
                Configuration config = rune.GetConfig("Rune", "Rune");
                Configuration config2 = rune.GetConfig("Hest", "Rune");
                config.SetValue("hej med dig2");
                config2.SetValue("2");
            }

            Console.WriteLine("hej");



            /*
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
            */

        }
    }

    /*
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
     */
}
