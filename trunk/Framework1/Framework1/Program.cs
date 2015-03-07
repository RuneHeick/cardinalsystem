using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileModules;
using FileModules.Event;

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

            Console.ReadKey();
        }

        private static void SettingChangedHandler(object sender, SettingEventArgs<int> e)
        {
            var s = e.Setting;

        }
    }
}
