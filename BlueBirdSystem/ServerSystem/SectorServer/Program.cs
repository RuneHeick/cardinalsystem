using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SectorServer
{
    class Program
    {

        private static MainController _mainController;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Server...");
            _mainController = new MainController(3030, 3031);
            Console.WriteLine("Server Started!");
            //_mainController.Tick();
                
            Process proc = Process.GetCurrentProcess();
            long test = proc.PrivateMemorySize64;
            float data = test / 1000000f;
            Console.WriteLine("Idle Memmory: "+data+"MB");

            var task = Task.Delay(1000000);
            task.Wait();
        }

    }
}
