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
using NetworkModules.Connection.Packet.Commands;

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
            var a = CommandCollection.Instance; 
            ConnectionManager RemoteManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9000));
            ConnectionManager conManager = new ConnectionManager(new IPEndPoint(IPAddress.Loopback, 9090));

            RemoteManager.RemoteConnectionCreated += NewConnection; 


            Connection connection = conManager.GetConnection(new IPEndPoint(IPAddress.Loopback, 9000)); 
            connection.OnStatusChanged += StatusChanged;

            Thread.Sleep(100);
            /*
            var packet1 = new NetworkPacket();
            var packet2 = new NetworkPacket();

            var command1 = cmdCollection.CreateCommand("TestDy", 0);
            var command2 = cmdCollection.CreateCommand("TestFi", 2);
            
            var element1 = new PacketElement(new byte[127], command1);
            var element2 = new PacketElement(new byte[2] { 3, 4 }, command2);

            packet1.Add(element1); 
            

            connection.Send(packet1);
            */
            Thread.Sleep(1000);

            

            Console.ReadKey();
            connection.Close();
        }

        private static void NewConnection(object sender, ConnectionEventArgs<Connection> e)
        {
            e.Connection.OnStatusChanged += StatusChanged;
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
