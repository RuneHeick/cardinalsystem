using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server3.Intercom.SharedFile.Files
{
    public class SystemFileIndexFile:BaseFile
    {

        List<SystemFileInfo> systemFileInfosfileInfos = new List<SystemFileInfo>();


        public override byte[] Data
        {
            get { return ToByte(); }
            set { FromByte(value); }
        }

        public void AddFileInfo(string name, UInt32 hash)
        {
            lock (systemFileInfosfileInfos)
            {
                SystemFileInfo info = systemFileInfosfileInfos.FirstOrDefault((o) => o.Name == name);
                if (info == null)
                {
                    info = new SystemFileInfo()
                    {
                        Name = name,
                        Hash = hash
                    };
                    systemFileInfosfileInfos.Add(info);
                }
                else
                {
                    info.Hash = hash; 
                }
            }
        }

        public UInt32 GetHash(string name)
        {
            lock (systemFileInfosfileInfos)
            {
                SystemFileInfo info = systemFileInfosfileInfos.FirstOrDefault((o) => o.Name == name);
                if (info != null)
                    return info.Hash;
                else
                 throw new InstanceNotFoundException("No File Named "+name);
            }
        }

        private void FromByte(byte[] value)
        {
            lock (systemFileInfosfileInfos)
            {
                int startIndex = 0;
                int i = 0;
                while (i < value.Length)
                {
                    i = Array.IndexOf(value, (byte)0, startIndex);
                    UInt32 hash = BitConverter.ToUInt32(value, i + 1);
                    string name = UTF8Encoding.UTF8.GetString(value, startIndex, i - startIndex);

                    SystemFileInfo info = new SystemFileInfo()
                    {
                        Name = name,
                        Hash = hash
                    };

                    systemFileInfosfileInfos.Add(info);

                    i += 5;
                    startIndex = i;
                }
            }
        }

        private byte[] ToByte()
        {
            List<byte> ret = new List<byte>(); 
            lock (systemFileInfosfileInfos)
            {
                foreach (SystemFileInfo info in  systemFileInfosfileInfos)
                {
                    byte[] name = UTF8Encoding.UTF8.GetBytes(info.Name);
                    ret.AddRange(name);
                    ret.Add(0);
                    byte[] hash = BitConverter.GetBytes(info.Hash);
                    ret.AddRange(hash);
                }
            }
            return ret.ToArray();
        }


        public class SystemFileInfo
        {
            public string Name { get; set; }
            public UInt32 Hash { get; set; }
        }
    }
}
