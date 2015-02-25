using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server3;
using Server3.Intercom.Network;
using Server3.Intercom.SharedFile;

namespace MulticastTestApp
{
    class Program
    {


        static void Main(string[] args)
        {

            

            var address = new IPEndPoint(IPAddress.Parse("192.168.87.103"), 5050);
            
            NIC network = new NIC(address);
            SharedFileManager fileManager = new SharedFileManager("Folder", address);
            
            int known = 0;

            while (true)
            {
                int count = network.KnownEndPoints.Length;
                if (known != count && count>0)
                {
                    known = count;
                    Console.WriteLine("Known added with: " + network.KnownEndPoints[count-1].ToString());
                }
                
                Thread.Sleep(5000);

                using (var file = fileManager.GetFile<BaseFile>("BigFile", address.Address, true))
                {
                    file.Data = new byte[100000];
                    file.Data[0] = (byte)(DateTime.Now.Ticks%255);
                    Console.WriteLine("Big " + file.Data[0]);
                }
                

            }



        }

    }
}
