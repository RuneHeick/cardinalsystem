using Server;
using Server.InterCom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkClasses
{
    class Program
    {

        static void Main(string[] args)
        {
            /*
            var item = TimeOut.Create<int>(10000, 2, Done);
            TimeOut.Create<int>(10000, 1, Done);
            
            TimeOut.Create<int>(30000, 3, Done);
            Thread.Sleep(2000);
            item.Touch();
             */
            Server.Server t = new Server.Server(5050); 
           
           
            //test.Start(new System.Net.IPEndPoint(IPAddress.Parse("192.2.2.2"), 5050));

            //MulticastManager manager3 = new MulticastManager();
            //manager3.Send(new byte[3]);

            Random RAN = new Random(); 
            while(true)
            {
                Thread.Sleep(100); 
                t.Test();
                if (RAN.Next(10) == 5)
                    Thread.Sleep(7000);
            }
            

            Thread.Sleep(Timeout.Infinite);
        }

        private static void Done(int obj)
        {
            Console.WriteLine("Done "+obj.ToString()); 
        }

        static void manager_OnMulticastRecived(byte[] arg1, System.Net.IPEndPoint arg2)
        {
            Console.WriteLine("Test"); 
        }
    }
}
