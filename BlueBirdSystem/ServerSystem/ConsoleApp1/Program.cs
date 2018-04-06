using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        const int NUM = 100000000;


        List<Action> workList = new List<Action>();

        static void Main(string[] args)
        {
            TestUnit[] units = new TestUnit[4000];
            for(int i = 0; i<units.Length; i++)
            {
                units[i] = new TestUnit();
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            foreach (var fu in units)
            {
                foreach (var cu in units)
                {
                    if (fu != cu)
                    {
                            fu.Action(cu);
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine("Time: " + stopwatch.ElapsedMilliseconds + " ms");

            Console.ReadKey();
        }


        static void MemoryTest()
        {
            var list = new List<byte[]>(NUM);
            for (ulong i = 0; i < NUM; i++)
            {
                list.Add(new byte[8 * 2]);
            }


            Process proc = Process.GetCurrentProcess();
            long test = proc.PrivateMemorySize64;
        }

    }
}
