using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace Server.NewInterCom.SharedSettings
{
    public class SettingManager
    {
        #region Static 

        private static string SettingsFolder = "Settings";
        private const int HashSize = 16; 

        private static Dictionary<IPAddress, SettingManager> Managers = new Dictionary<IPAddress, SettingManager>();
        private static HashAlgorithm hash = HashAlgorithm.Create("MD5");   


        static SettingManager()
        {
            DirectoryInfo settings = new DirectoryInfo(SettingsFolder); 
            if(!settings.Exists)
            {
                settings.Create();
            }
        }

        public static SettingManager GetSettingManager(IPAddress address)
        {
            if (Managers.ContainsKey(address))
            {
                return Managers[address];
            }
            return null;
        }
       
        public static void RegistreIP(IPAddress address)
        {
            if (!Managers.ContainsKey(address))
            {
                Managers.Add(address, new SettingManager(address));
            }
        }


        #endregion

        #region Instance

        private string InstanceFolder { get; set; }

        private SettingManager(IPAddress address)
        {
            InstanceFolder = BitConverter.ToString(address.GetAddressBytes()).Replace("-", "");
            DirectoryInfo dir = new DirectoryInfo(SettingsFolder + "/" + InstanceFolder);
            if (!dir.Exists)
                dir.Create(); 
            
            Managers.Add(address, this); 
        }

        

        

        public async Task<T> GetSetting<T>(string Name) where T : ISetting, new()
        {
            byte[] data = await GetFile(Name);
            if (data != null)
            {
                T item = new T();
                item.Name = Name;
                item.Data = data;
                item.OnDataChanged += item_OnDataChanged;

                return item;
            }
            return null;
        }

        public async Task<T> CreateSetting<T>(string Name) where T : ISetting, new()
        {
            T item = new T();
            item.Name = Name;
            byte[] data = await GetFile(Name);
            if (data != null)
            {
                item.Data = data;
            }

            item.OnDataChanged += item_OnDataChanged;

            return item;
        }

        public async Task<byte[]> GetHash(string Name)
        {
            FileInfo file = new FileInfo(SettingsFolder + "/" + InstanceFolder + "/" + Name);
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

        private async Task<byte[]> GetFile(string Name)
        {
            byte[] data = null;
            FileInfo file = new FileInfo(SettingsFolder + "/" + InstanceFolder + "/" + Name);
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
                byte[] hashbyte = hash.ComputeHash(obj.Data);
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
