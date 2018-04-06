using MatrixSystem;
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

            for (int a = 0; a < ITERATIONS; a++)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                syncManager.UpdateGameLoop();

                watch.Stop();
                Console.WriteLine("Elapse: " + watch.ElapsedMilliseconds + " ms");
            }

            Console.ReadKey();
        }

        public static void InitTest()
        {
            Random random = new Random();
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
                for(uint k = 1+i; k<500+i; k++)
                     client.Add(k);
                
                syncManager.AddSubscription(client);
            }
        }

        private static void Con_OnItemZoneChanged(MatrixSystem.GameObject gameObject, MatrixSystem.ObjectMatrix.Types.PositionValue position, MatrixSystem.ZoneTypes zoneType, MatrixSystem.ZoneTypes oldZone)
        {
            Console.WriteLine("ID \t"+gameObject.ObjectID.ToString()+"\t MOVED FROM "+oldZone.ToString()+" MOVE TO: "+zoneType.ToString());
        }
    }
}
