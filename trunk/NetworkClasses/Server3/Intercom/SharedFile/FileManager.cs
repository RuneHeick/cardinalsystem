﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.InterCom;
using Server3.Intercom.Network;
using Server3.Intercom.Network.Packets;
using Server3.Intercom.SharedFile.Files;
using Server3.Utility;

namespace Server3.Intercom.SharedFile
{
    public class SharedFileManager
    {
        private readonly string _baseDirectory;
        private readonly IPEndPoint _me;
        private readonly Dictionary<IPAddress, LocalFileManager> _knownFileManagers = new Dictionary<IPAddress, LocalFileManager>();

        public SharedFileManager(string baseDirectory, IPEndPoint me  )
        {
            _baseDirectory = baseDirectory;
            _me = me;
            InitDirectory();

            EventBus.Subscribe<ClientFoundEvent>(NewClientFount);
        }

        #region Setup

        private void InitDirectory()
        {
            DirectoryInfo subDir = new DirectoryInfo(_baseDirectory + "/" + _me.Address);
            if (!subDir.Exists)
            {
                subDir.Create();
            }

            LocalFileManager localFileManager = new LocalFileManager(_me, subDir);
            localFileManager.SetupLocal();
            lock (_knownFileManagers)
            {
                _knownFileManagers.Add(_me.Address, localFileManager);
            }


        }

        private void NewClientFount(ClientFoundEvent client)
        {
            lock (_knownFileManagers)
            {
                DirectoryInfo folder = new DirectoryInfo(_baseDirectory + "/" + client.Address.Address);
                if(!folder.Exists)
                    folder.Create();
                if (!_knownFileManagers.ContainsKey(client.Address.Address))
                {
                    LocalFileManager localFileManager = new LocalFileManager(client.Address, folder);
                    _knownFileManagers.Add(client.Address.Address, localFileManager);
                    TimeOut.Create<object>(1000, null, (o) => localFileManager.Setup());


                }
            }
        }

        #endregion

        #region ReciveFile 

        public T GetFile<T>(string name, IPAddress address, bool create = false) where T : BaseFile, new()
        {
            LocalFileManager manager = null;
            lock (_knownFileManagers)
            {
                if (_knownFileManagers.ContainsKey(address))
                    manager = _knownFileManagers[address];
            }

            if (manager != null)
            {
                return manager.GetFile<T>(name, create); 
            }

            return null; 
        }

        public byte[] GetHash(string name, IPAddress address)
        {
            LocalFileManager manager = null;
            lock (_knownFileManagers)
            {
                if (_knownFileManagers.ContainsKey(address))
                    manager = _knownFileManagers[address];
            }

            if (manager != null)
            {
                return manager.GetHash(name);
            }

            return null;
        } 

        #endregion

        
    }


    class LocalFileManager
    {
        private readonly IPEndPoint _address;
        private readonly DirectoryInfo _folder;
        private readonly Dictionary<string, BaseFile> _openFiles = new Dictionary<string, BaseFile>();

        private readonly Dictionary<byte, ReciveFile> _recivedFiles = new Dictionary<byte, ReciveFile>(); 

        public LocalFileManager(IPEndPoint address, DirectoryInfo folder)
        {
            _address = address;
            _folder = folder;
        }

        public void Setup()
        {
            EventBus.Subscribe<NetworkPacket>(NetworkPacketRecived, (p) => p.Address.Address.Equals(_address.Address));
            var filecontainor = GetFile<SystemFileIndexFile>("FileInfo");
            if (filecontainor == null)
                RequestFile("FileInfo");
        }

        public void SetupLocal()
        {
            var filecontainor = GetFile<SystemFileIndexFile>("FileInfo", true);
            filecontainor.AddFileInfo("Test",52200);
            filecontainor.Dispose();
            EventBus.Subscribe<NetworkPacket>(NetworkPacketRecived, (p) => p.Command == (byte)InterComCommands.PacketInfo);
        }

        #region FileSystem

        public T GetFile<T>(string name, bool create = false) where T : BaseFile, new()
        {
            T file = null;
            lock (_openFiles)
            {
                if (_openFiles.ContainsKey(name))
                    return _openFiles[name] as T;

                if (File.Exists(_folder.FullName + "/" + name) || create)
                    file = CreateOrOpenFile<T>(name);

                if(file != null)
                    _openFiles.Add(file.Name, file);
            }
            return file; 
        }

        public bool Exists(string name)
        {
            return File.Exists(_folder.FullName + "/" + name); 
        }

        public byte[] GetHash(string name)
        {
            lock (_openFiles)
            {
                if (_openFiles.ContainsKey(name))
                    return _openFiles[name].Hash;
            }
            FileInfo file = new FileInfo(_folder.FullName + "/" + name);
            if (file.Exists)
            {
                using (FileStream r = file.OpenRead())
                {
                    r.Seek(file.Length - Crc32.HashSize, SeekOrigin.Begin);
                    byte[] hash = new byte[Crc32.HashSize];
                    r.Read(hash, 0, Crc32.HashSize);
                    r.Close();
                    return hash;
                }
            }
            return null;
        }

        private T CreateOrOpenFile<T>(string name) where T : BaseFile, new()
        {
            lock (_folder)
            {
                T file = null;
                FileInfo fileInfo = new FileInfo(_folder.FullName + "/" + name);
                if (fileInfo.Exists)
                {
                    try
                    {
                        using (FileStream r = fileInfo.OpenRead())
                        {
                            long length = fileInfo.Length - Crc32.HashSize;
                            byte[] data = new byte[length];
                            byte[] hash = new byte[Crc32.HashSize];
                            r.Read(data, 0, (int)length);
                            r.Read(hash, 0, Crc32.HashSize);
                            r.Close();

                            file = new T()
                            {
                                Data = data,
                                Name = name,
                                Hash = hash,
                                CloseAction = SaveFile
                            };

                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                else
                {
                    file = new T()
                    {
                        Data = new byte[0],
                        Name = name,
                        CloseAction = SaveFile
                    };
                }

                return file; 
            }
        }

        private void SaveFile(BaseFile file)
        {
            lock (_openFiles)
            {
                if (_openFiles.ContainsKey(file.Name))
                    _openFiles.Remove(file.Name);

                try
                {
                    byte[] hashbyte = Crc32.UInt32ToBigEndianBytes(Crc32.CalculateHash(file.Data));
                    using (FileStream w = File.OpenWrite(_folder.FullName+"/" + file.Name))
                    {
                        w.Write(file.Data, 0, file.Data.Length);
                        w.Write(hashbyte, 0, hashbyte.Length);
                        w.Close();
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

        #region Network 

        private void NetworkPacketRecived(NetworkPacket packet)
        {
            switch (packet.Command)
            {
                case (byte)InterComCommands.PacketInfo:
                    InfoPacketRecived(packet);
                    break;
                case (byte)InterComCommands.PacketRecive:
                    FilePacketRecived(packet);
                    break;
            }
        }

        private void FilePacketRecived(NetworkPacket packet)
        {

            var id = packet[0];
            bool done = (packet[1] & 0x80) > 0;
            lock (_recivedFiles)
            {
                if (_recivedFiles.ContainsKey(id))
                {
                    _recivedFiles[id].Add(packet);
                    if (done)
                    {

                        var fi = _recivedFiles[id];
                        _recivedFiles.Remove(id);

                        FileReciveDone(fi);
                    }
                }
            }
        }

        private void FileReciveDone(ReciveFile recivedFile)
        {
            byte[] file = new byte[recivedFile.Size];
            int startIndex = 0; 
            int session = 0;
            
            while (true)
            {
                var p = recivedFile.Packets.FirstOrDefault((o) => (o[1] & 0x7F) == session);
                NetworkPacket.Copy(file, startIndex, p, 2, p.PayloadLength - 2);
                startIndex += p.PayloadLength - 2;
                session++; 
                if((p[1] & 0x80)>0)
                    break;
            }
            
            using (var fileinfo = CreateOrOpenFile<BaseFile>(recivedFile.Name))
            {
                fileinfo.Data = file; 
            }
        }

        public void RequestFile(string name)
        {
            NetworkRequest rq = NetworkRequest.CreateSignal(1 + name.Length, PacketType.Tcp);
            lock (_recivedFiles)
            {
                byte i = 0;
                for (; i < 0xFF; i++)
                {
                    if (!_recivedFiles.ContainsKey(i))
                        break;
                }
                rq.Packet.Address = _address;
                rq.Packet.Command = (byte)InterComCommands.PacketInfo;
                _recivedFiles.Add(i, new ReciveFile() {Name = name});
                rq.Packet[0] = i;
                for (int z = 0; z < name.Length; z++)
                {
                    rq.Packet[1 + z] = (byte) name[z];
                }
            }
            EventBus.Publich(rq);
        }

        private void InfoPacketRecived(NetworkPacket packet)
        {
            var id = packet[0];
            var len = packet.PayloadLength - 1;
            StringBuilder sb = new StringBuilder(len);
            for (int i = 1; i < len+1; i++)
            {
                sb.Append((char) packet[i]);
            }
            string name = sb.ToString();
            SendFile(id, name, packet.Address); 
        }

        private void SendFile(byte id, string name, IPEndPoint iPEndPoint)
        {
            BaseFile file = GetFile<BaseFile>(name);
            if (file != null)
            {
                var data = file.Data;
                int size = 0;
                int session = 0;
                bool done = false;

                while (size < data.Length)
                {
                    int packetLength = (data.Length - size) > 1000 ? 1000 : data.Length - size;
                    if (size + packetLength == data.Length)
                        done = true;

                    NetworkRequest rq = NetworkRequest.CreateSignal(packetLength+2, PacketType.Tcp);
                    NetworkPacket.Copy(rq.Packet, 2, data, size, packetLength);
                    rq.Packet.Address = iPEndPoint; 
                    rq.Packet[0] = id;
                    rq.Packet[1] = (byte)((session++) | (done ? 0x80 : 0x00));
                    rq.Packet.Command = (byte)InterComCommands.PacketRecive;
                    EventBus.Publich(rq);

                    size += packetLength;
                }
            }
        }

        #endregion


        private class ReciveFile
        {
            public string Name { get; set; }

            public int Size { get; private set; }

            public List<NetworkPacket> Packets = new List<NetworkPacket>();

            public void Add(NetworkPacket packet)
            {
                Size += packet.PayloadLength-2;
                Packets.Add(packet);
            }

        }


       
    }
}