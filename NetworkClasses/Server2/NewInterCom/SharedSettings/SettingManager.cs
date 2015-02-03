using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Server.InterCom;
using Server.NewInterCom.Com;
using Server2.Utility;

namespace Server.NewInterCom.SharedSettings
{
    public class SettingManager
    {
        #region Static 

        private const string SettingsFolder = "Settings";
        private const int HashSize = 16; 

        private static readonly Dictionary<IPAddress, SettingManager> Managers = new Dictionary<IPAddress, SettingManager>();
        private static readonly HashAlgorithm Hash = HashAlgorithm.Create("MD5");

        private static readonly EventManager<object, SettingManager> NewSettingEventManager;
        private static readonly Action<object, SettingManager> PublishFunction;

        private static ComLink _link;
        private static ComLink Link
        {
            get { return _link; }
            set
            {
                _link = value;
                _link.PacketRecived += NetworkPacketRecived;
            }
        }

        static SettingManager()
        {
            DirectoryInfo settings = new DirectoryInfo(SettingsFolder); 
            if(!settings.Exists)
            {
                settings.Create();
            }

            //setup Publish Subscriber 
            EventManagerSetupInfo<object, SettingManager> info = new EventManagerSetupInfo<object, SettingManager>();
            NewSettingEventManager = new EventManager<object, SettingManager>(info);
            PublishFunction = info.Publish; 
        }

        public static SettingManager GetSettingManager(IPAddress address)
        {
            if (Managers.ContainsKey(address))
            {
                return Managers[address];
            }
            return null;
        }

        public static SettingManager CreateManager(IPAddress address)
        {
            SettingManager manager; 
            if (!Managers.ContainsKey(address))
            {
                manager = new SettingManager(address);
                Managers.Add(address, manager);
            }
            else
            {
                manager = Managers[address]; 
            }
            return manager; 
        }

        public static SettingManager CreateManager(ComLink link, PeriodicManager pManager)
        {
            SettingManager manager;
            if (!Managers.ContainsKey(link.Address.Address))
            {
                manager = new SettingManager(link.Address.Address);
                Managers.Add(link.Address.Address, manager);
            }
            else
            {
                manager = Managers[link.Address.Address];
            }

            Link = link;

            pManager.PeriodicMessageAddedEventManager.Subscribe(PeriodicMessageAdded, CheckPeriodicManager);

            return manager;
        }

        public static void Subscribe<T>(Action<T, SettingManager> eventHandler, string fileName) where T : ISetting, new()
        {
            Action<object, SettingManager> action = (o, a) =>
            {
                var item = o as T;
                if (item != null)
                    eventHandler(item, a); 
            };

            Func<object, SettingManager, bool> conFunc = null; 
            if (fileName != null)
            {
                conFunc = (o, a) =>
                {
                    var item = o as T;
                    if (item != null)
                        return item.Name == fileName;
                    return false; 
                };
            }

            NewSettingEventManager.Subscribe(action, conFunc);

        }


        private static void NetworkPacketRecived(InterCom.InternalNetworkCommands command, byte[] packet, IPAddress address, SendType type)
        {
            if (command == InternalNetworkCommands.FileTransfer)
            {
                

            }
            else if (command == InternalNetworkCommands.FileTransfer)
            {
                
            }
        }


        private static bool CheckPeriodicManager(PeriodicManager.PeriodicMessage pManager, IPAddress address)
        {

            return false;
        }

        private static void PeriodicMessageAdded(PeriodicManager.PeriodicMessage message, IPAddress address)
        {
            


        }


        #endregion


        #region Instance

        private string InstanceFolder { get; set; }

        public IPAddress Address { get; private set; }

        private SettingManager(IPAddress address)
        {
            InstanceFolder = BitConverter.ToString(address.GetAddressBytes()).Replace("-", "");
            DirectoryInfo dir = new DirectoryInfo(SettingsFolder + "/" + InstanceFolder);
            if (!dir.Exists)
                dir.Create(); 
            
            Managers.Add(address, this);
            Address = address; 
        }

        public bool HasSetting(string name)
        {
            FileInfo file = new FileInfo(SettingsFolder + "/" + InstanceFolder + "/" + name);
            return file.Exists; 
        }

        public async Task<T> GetSetting<T>(string name) where T : ISetting, new()
        {
            byte[] data = await GetFile(name);
            if (data != null)
            {
                T item = new T();
                item.Name = name;
                item.Data = data;
                item.OnDataChanged += item_OnDataChanged;

                return item;
            }
            return null;
        }

        public async Task<T> CreateSetting<T>(string name) where T : ISetting, new()
        {
            T item = new T {Name = name};
            byte[] data = await GetFile(name);
            if (data != null)
            {
                item.Data = data;
            }
            else
            {
                NewSettingEventManager.Publish(item, Address);
            }
            item.OnDataChanged += item_OnDataChanged;

            return item;
        }

        public async Task<byte[]> GetHash(string name)
        {
            FileInfo file = new FileInfo(SettingsFolder + "/" + InstanceFolder + "/" + name);
            if (file.Exists)
            {
                using (FileStream r = file.OpenRead())
                {
                    r.Seek(file.Length - HashSize, SeekOrigin.Begin);
                    byte[] hash = new byte[HashSize];
                    await r.ReadAsync(hash, 0, HashSize);
                    r.Close();
                    return hash;
                }
            }
            return null; 
        }

        private async Task<byte[]> GetFile(string name)
        {
            byte[] data = null;
            FileInfo file = new FileInfo(SettingsFolder + "/" + InstanceFolder + "/" + name);
            if (file.Exists)
            {
                try
                {
                    using (FileStream r = file.OpenRead())
                    {
                        long length = file.Length - HashSize;
                        data = new byte[length];
                        await r.ReadAsync(data, 0, (int)length);
                        r.Close();
                    }
                }
                catch
                {

                }
            }
            return data;
        }

        private async void item_OnDataChanged(ISetting obj)
        {
            try
            {
                byte[] hashbyte = Hash.ComputeHash(obj.Data);
                using (FileStream w = File.OpenWrite(SettingsFolder + "/" + InstanceFolder + "/" + obj.Name))
                {
                    await w.WriteAsync(obj.Data, 0, obj.Data.Length);
                    await w.WriteAsync(hashbyte, 0, hashbyte.Length);
                    w.Close();
                }
            }
            catch
            {

            }
        }

        #endregion

    }
}
