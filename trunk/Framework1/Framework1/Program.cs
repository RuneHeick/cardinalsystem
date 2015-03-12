using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileModules;
using FileModules.Event;
using Intercom;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;
using NetworkModules.Connection.Packet.Commands;

namespace Framework1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(SystemTester.GetStatus());

            /*
            ISettingManager manager = new SettingManager("Settings.txt");

            var s1 = manager.GetOrCreateSetting("Test", "Start", "Hest");
            var s2 = manager.GetOrCreateSetting("Test2", "Start", 3);
            var s3 = manager.GetSetting<string>("Test4", "Start");
            var s4 = manager.GetSetting<string>("Test", "Start");

            s2.SettingChanged += SettingChangedHandler;
            s1.Value = "Hop";
            */


            CommandCollection.Instance.CreateProtocolDefinition();

            var col = CommandCollection.Instance.GetCollection();

            CommandCollection.Instance.CheckCollection(col);


            ConnectionManager RemoteManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9000));
            ConnectionManager conManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9090));

            RemoteManager.RemoteConnectionCreated += NewConnection; 


            Connection connection = conManager.GetConnection(new IPEndPoint(IPAddress.Loopback, 9000), 5000); 
            connection.OnStatusChanged += StatusChanged;

            Thread.Sleep(100);

            var packet1 = new NetworkPacket();
            var element1 = new DummyElement() {Data = new byte[] {1, 2, 3, 4}};
            packet1.Add(element1);
            packet1.Add(element1);
            packet1.Add(element1);
           

            while (true)
            {
                connection.Send(packet1);

                Console.ReadKey();    
            }
            
            
        }

        private static void NewConnection(object sender, ConnectionEventArgs<Connection> e)
        {
            e.Connection.OnStatusChanged += StatusChanged;
            e.Connection.Add(new DummyProtocol(typeof(DummyElement)));
        }

        private static void StatusChanged(object sender, ConnectionEventArgs e)
        {
            Connection con = (Connection) (sender); 
            Console.WriteLine("Status Changed of "+ con.RemoteEndPoint+" to: "+e.Status.ToString());
        }

        private static void SettingChangedHandler(object sender, SettingEventArgs<int> e)
        {
            var s = e.Setting;
        }
    }
}
