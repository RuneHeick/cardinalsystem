using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileModules;
using FileModules.Event;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;

namespace Framework1
{
    class Program
    {
        static void Main(string[] args)
        {

            ISettingManager manager = new SettingManager("Settings.txt");

            var s1 = manager.GetOrCreateSetting("Test", "Start", "Hest");
            var s2 = manager.GetOrCreateSetting("Test2", "Start", 3);
            var s3 = manager.GetSetting<string>("Test4", "Start");
            var s4 = manager.GetSetting<string>("Test", "Start");

            s2.SettingChanged += SettingChangedHandler;
            s1.Value = "Hop";


            CommandCollection cmdCollection = new CommandCollection();
            NetworkPacket.SetCommandCollection(cmdCollection);
            ConnectionManager RemoteManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9000));
            ConnectionManager conManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9090));

            RemoteManager.RemoteConnectionCreated += NewConnection; 


            Connection connection = conManager.GetConnection(new IPEndPoint(IPAddress.Loopback, 9000)); 
            connection.OnStatusChanged += StatusChanged;

            Thread.Sleep(100);

            

            connection.Send(new NetworkPacket());

            Thread.Sleep(100);

            connection.Close();

            Console.ReadKey();
        }

        private static void NewConnection(object sender, ConnectionEventArgs<Connection> e)
        {
            e.Connection.OnStatusChanged += StatusChanged;
            e.Connection.OnPacketRecived += PacketRecived;
        }

        private static void PacketRecived(object sender, PacketEventArgs e)
        {
            
        }

        private static void StatusChanged(object sender, ConnectionEventArgs e)
        {
            Connection con = (Connection) (sender); 
            Console.WriteLine("Setting Changed of "+ con.RemoteEndPoint+" to: "+e.Status.ToString());
        }

        private static void SettingChangedHandler(object sender, SettingEventArgs<int> e)
        {
            var s = e.Setting;

        }
    }
}
