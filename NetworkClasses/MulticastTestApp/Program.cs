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
using Server3.Intercom.PeriodicSyncItem;
using Server3.Intercom.PeriodicSyncItem.File;
using Server3.Intercom.SharedFile;

namespace MulticastTestApp
{
    class Program
    {

        private static BaseFile bigFile = null; 

        static void Main(string[] args)
        {


            PeriodicTTFile test = new PeriodicTTFile();
            test.Add("Rune",4);
            test.Add("Maja", 5);

            byte[] data = test.Data;

            test = new PeriodicTTFile();
            test.Data = data; 

            var address = new IPEndPoint(IPAddress.Parse("192.168.87.101"), 5050);
            
            NIC network = new NIC(address);
            FileManager.Me = address; 
            PeriodicSyncManager pSync = new PeriodicSyncManager(address);
 


            int known = 0;

            FileRequest rq = FileRequest.CreateFileRequest<BaseFile>("The big Testfile.txt", address, gotFile);
            EventBus.Publich(rq);
            Thread.Sleep(100);
            bigFile.Data = new byte[3300];
            for (int i = 0; i < bigFile.Data.Length; i++)
            {
                bigFile.Data[i] = (byte) ((i%25) + 65);
            }
            bigFile.Dispose();

            while (true)
            {
                int count = network.KnownEndPoints.Length;
                if (known != count && count>0)
                {
                    known = count;
                    Console.WriteLine("Known added with: " + network.KnownEndPoints[count-1].ToString());
                }
                
                Thread.Sleep(5000);
                
            }



        }

        private static void gotFile(BaseFile file)
        {
            bigFile = file; 
        }

    }
}
