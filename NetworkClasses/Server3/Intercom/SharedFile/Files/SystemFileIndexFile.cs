﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server3.Intercom.SharedFile.Files
{
    public class SystemFileIndexFile:BaseFile
    {

        public List<SystemFileInfo> filesInformation = new List<SystemFileInfo>();

        public override byte[] Data
        {
            get { return ToByte(); }
            set { FromByte(value); }
        }

        public void AddFileInfo(string name, UInt32 hash)
        {
            lock (filesInformation)
            {
                SystemFileInfo info = filesInformation.FirstOrDefault((o) => o.Name == name);
                if (info == null)
                {
                    info = new SystemFileInfo()
                    {
                        Name = name,
                        Hash = hash
                    };
                    filesInformation.Add(info);
                    OnFileChanged(name); 
                }
                else
                {
                    if (info.Hash != hash)
                    {
                        info.Hash = hash;
                        OnFileChanged(name);
                    }
                }
            }
        }

        public UInt32 GetHash(string name)
        {
            lock (filesInformation)
            {
                SystemFileInfo info = filesInformation.FirstOrDefault((o) => o.Name == name);
                if (info != null)
                    return info.Hash;
                else
                 throw new InstanceNotFoundException("No File Named "+name);
            }
        }

        private void FromByte(byte[] value)
        {
            lock (filesInformation)
            {
                int startIndex = 0;
                int i = 0;
                while (i < value.Length)
                {
                    i = Array.IndexOf(value, (byte)0, startIndex);
                    UInt32 hash = BitConverter.ToUInt32(value, i + 1);
                    string name = UTF8Encoding.UTF8.GetString(value, startIndex, i - startIndex);

                    AddFileInfo(name, hash);

                    i += 5;
                    startIndex = i;
                }
            }
        }

        private byte[] ToByte()
        {
            List<byte> ret = new List<byte>(); 
            lock (filesInformation)
            {
                foreach (SystemFileInfo info in  filesInformation)
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

        public event Action<string> FileChanged; 


        public class SystemFileInfo
        {
            public string Name { get; set; }
            public UInt32 Hash { get; set; }
        }

        protected virtual void OnFileChanged(string name)
        {
            var handler = FileChanged;
            if (handler != null) handler(name);
        }
    }
}
