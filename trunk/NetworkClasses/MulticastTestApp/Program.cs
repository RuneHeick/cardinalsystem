using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Network;

namespace MulticastTestApp
{
    class Program
    {


        static void Main(string[] args)
        {

            NIC network = new NIC(new IPEndPoint(IPAddress.Any, 5050));

            int known = 0;

            while (true)
            {
                int count = network.KnownEndPoints.Length;
                if (known != count && count>0)
                {
                    known = count;
                    Console.WriteLine("Known added with: " + network.KnownEndPoints[count-1].ToString());
                }
            }



        }
    }
}
