using MatrixSystem;
using MatrixSystem.Network;
using MatrixSystem.ObjectCreation;
using MatrixSystem.ObjectMatrix.Types;
using MatrixSystem.SyncManagers;
using System;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApp2
{
    class Program
    {
        const int ITERATIONS = 25;

        static SyncManager syncManager = new SyncManager();

        static void Main(string[] args)
        {
            syncManager.controller.OnItemZoneChanged += Con_OnItemZoneChanged;

            InitTest();
            ulong totalBytes = 0; 
            for (int a = 0; a < ITERATIONS; a++)
            {
                Stopwatch watch = new Stopwatch();
                VirtualSocket.BytesGennerated = 0;  
                watch.Start();

                syncManager.UpdateGameLoop();

                watch.Stop();
                totalBytes += VirtualSocket.BytesGennerated;
                Console.WriteLine("Elapse: " + watch.ElapsedMilliseconds + " ms\t Bytes: "+ VirtualSocket.BytesGennerated);
            }
            Console.WriteLine("Avg Bytes/sec: " + (totalBytes/ ITERATIONS)*2);

            Console.ReadKey();
        }

        public static void InitTest()
        {
            Random random = new Random();

            UInt16 hash = GameObjectCreator.getObjectHash(typeof(TestGameObject));
            GameObject created = GameObjectCreator.CreateNewObject(hash);

            for (uint i = 0; i < 8; i++)
            {
                SyncClient client = new SyncClient(new MatrixSystem.Network.VirtualSocket());
                syncManager.AddBorderSubscription(client, (ZoneTypes)((uint)ZoneTypes.IN_SYNC_ZONE_UPPER_LEFT+i));
            }

            for (int i = 0; i < 4000; i++)
            {
                int x = random.Next(1, 100);
                int y = random.Next(1, 100);

                GameObject obj = new GameObject();
                obj.Position.X = ((GridController.sectorSize / GridController.size) * x) - 0.1f;
                obj.Position.Y = ((GridController.sectorSize / GridController.size) * y) - 0.1f;

                syncManager.Add(obj);
            }


            for (uint i = 0; i < 100; i++)
            {
                SyncClient client = new SyncClient(new MatrixSystem.Network.VirtualSocket());
                for(uint k = 1+i; k<=200+i; k++)
                     client.Add(k);
                
                syncManager.AddSubscription(client);
            }
        }

        private static void Con_OnItemZoneChanged(MatrixSystem.GameObject gameObject, MatrixSystem.ObjectMatrix.Types.PositionValue position, MatrixSystem.ZoneTypes zoneType, MatrixSystem.ZoneTypes oldZone)
        {
            //Console.WriteLine("ID \t"+gameObject.ObjectID.ToString()+"\t MOVED FROM "+oldZone.ToString()+" MOVE TO: "+zoneType.ToString());
        }
    }
}
