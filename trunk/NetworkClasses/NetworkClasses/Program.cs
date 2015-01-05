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

            Server.Server t = new Server.Server(5050); 
           
           
            //test.Start(new System.Net.IPEndPoint(IPAddress.Parse("192.2.2.2"), 5050));

            //MulticastManager manager3 = new MulticastManager();
            //manager3.Send(new byte[3]);
            Thread.Sleep(15000); 
            
        }

        static void manager_OnMulticastRecived(byte[] arg1, System.Net.IPEndPoint arg2)
        {
            Console.WriteLine("Test"); 
        }
    }
}
