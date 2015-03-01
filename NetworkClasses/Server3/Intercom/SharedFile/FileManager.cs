using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Win32.SafeHandles;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Errors;
using Server3.Intercom.Network;
using Server3.Intercom.Network.Packets;
using Server3.Intercom.SharedFile;
using Server3.Intercom.SharedFile.Files;
using Server3.Utility;

namespace Server3.Intercom.SharedFile
{
    internal class FileManager
    {
        #region Static

        public const string MainDirectory = "DataManager";
        private const string HashIndexFileName = "FileInfo";
        private const int FilePacketMaxSize = 1000; 

        private static readonly SafeCollection<IPAddress, FileManager> FileManagers = new SafeCollection<IPAddress, FileManager>(new IPEqualityComparer());
        private static readonly Dictionary<byte, ReciveFile> _recivedFiles = new Dictionary<byte, ReciveFile>();
        private static IPEndPoint _me = null;
        private static byte _fileReciveSession = 1;

        public static IPEndPoint Me
        {
            get { return _me; }
            set
            {
                if (_me == null && value != null)
                {
                    _me = value;
                    Start();
                }
            }
        }

        static FileManager()
        {
            Me = null; 
            DirectoryInfo dir = new DirectoryInfo(MainDirectory);
            if (!dir.Exists)
                dir.Create();
        }

        private static void Start()
        {
            SubscribeOnEvents();
            FileManager me = new FileManager(Me);
        }

        #region Network

        private static void NetworkFilePartRecived(NetworkPacket packet)
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
                        fi.MaxWaitTimeout.Calcel();
                        _recivedFiles.Remove(id);

                        FileReciveDone(fi, packet.Address.Address);
                    }
                }
            }
        }

        private static void FileReciveDone(ReciveFile recivedFile, IPAddress address)
        {

            byte[] file = new byte[recivedFile.Size];
            int startIndex = 0;
            int session = 0;


            while (true)
            {
                var p = recivedFile.Packets.FirstOrDefault((o) => (o[1] & 0x7F) == session);
                if (p != null)
                {
                    NetworkPacket.Copy(file, startIndex, p, 2, p.PayloadLength - 2);
                    startIndex += p.PayloadLength - 2;
                    session++;
                    if ((p[1] & 0x80) > 0)
                        break;
                }
                else
                {
                    Console.WriteLine("File: " + recivedFile.Name + " cannot be assembled");
                    return;
                }
            }

            if (FileManagers.ContainsFile(address))
            {
                var manager = FileManagers[address];
                manager.Update(recivedFile.Name, file); 
            }
        }

        private static void NetworkInfoRecived(NetworkPacket packet)
        {
            var id = packet[0];
            var len = packet.PayloadLength - 1;
            StringBuilder sb = new StringBuilder(len);
            for (int i = 1; i < len + 1; i++)
            {
                sb.Append((char)packet[i]);
            }
            string name = sb.ToString();
            SendFile(id, name, packet.Address);
        }

        private static void SendFile(byte id, string name, IPEndPoint iPEndPoint)
        {
            var me = FileManagers[Me.Address];
            var data = me.GetFileData(name);
            if (data != null)
            {
                try
                {
                        int size = 0;
                        int session = 0;
                        bool done = false;
                        Console.WriteLine("File Sendt To: " + iPEndPoint.Address);

                        while (size < data.Length)
                        {
                            int packetLength = (data.Length - size) > FilePacketMaxSize
                                ? FilePacketMaxSize
                                : data.Length - size;
                            if (size + packetLength == data.Length)
                                done = true;

                            NetworkRequest rq = NetworkRequest.CreateSignal(packetLength + 2, PacketType.Tcp);
                            NetworkPacket.Copy(rq.Packet, 2, data, size, packetLength);
                            rq.Packet.Address = iPEndPoint;
                            rq.Packet[0] = id;
                            rq.Packet[1] = (byte) ((session++) | (done ? 0x80 : 0x00));
                            rq.Packet.Command = (byte) InterComCommands.PacketRecive;

                            EventBus.Publich(rq, false);


                            size += packetLength;
                        }
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private static void RequestFile(string name, IPEndPoint endpoint, byte retransmit = 0)
        {
            NetworkRequest rq = NetworkRequest.CreateSignal(1 + name.Length, PacketType.Tcp);
            lock (_recivedFiles)
            {
                byte i = _fileReciveSession;
                for (; i != _fileReciveSession - 1; i = (byte)((i + 1) % 0xFF))
                {
                    if (!_recivedFiles.ContainsKey(i))
                    {
                        _fileReciveSession = (byte)(i + 1);
                        break;
                    }

                }
                rq.Packet.Address = endpoint;
                rq.Packet.Command = (byte)InterComCommands.PacketInfo;
                ReciveFile rfile = new ReciveFile() { Name = name, Session = i, Retransmit = retransmit };
                rfile.MaxWaitTimeout = TimeOut.Create(2000, rfile, (o) =>FileRequestTimeOut(o, endpoint));

                _recivedFiles.Add(i, rfile);
                rq.Packet[0] = i;
                for (int z = 0; z < name.Length; z++)
                {
                    rq.Packet[1 + z] = (byte)name[z];
                }
            }
            EventBus.Publich(rq);
        }

        private static void FileRequestTimeOut(ReciveFile obj, IPEndPoint endPoint)
        {
            lock (_recivedFiles)
            {
                if (_recivedFiles.ContainsKey(obj.Session))
                {
                    _recivedFiles.Remove(obj.Session);

                    if (obj.Retransmit < 3)
                    {
                        RequestFile(obj.Name, endPoint, ++obj.Retransmit);
                    }
                }
            }
        }

        #endregion


        #region Setup 

        private static void SubscribeOnEvents()
        {
            EventBus.Subscribe<ClientFoundEvent>(NewClientFount);
            EventBus.Subscribe<FileRequest>(NewFileRequest);

            EventBus.Subscribe<NetworkPacket>(NetworkInfoRecived,(p) => p.Command == (byte)InterComCommands.PacketInfo);
            EventBus.Subscribe<NetworkPacket>(NetworkFilePartRecived,(p) => p.Command == (byte)InterComCommands.PacketRecive);
        }

        private static void NewClientFount(ClientFoundEvent clientinfo)
        {
            lock (FileManagers)
            {
                if (!FileManagers.ContainsFile(clientinfo.Address.Address))
                {
                    var localFileManager = new FileManager(clientinfo.Address);
                }
            }
        }


        #endregion


        #region GetFile

        private static void NewFileRequest(FileRequest fileRequest)
        {
            FileManager manager = null;
            lock (FileManagers)
            {
                if (FileManagers.ContainsFile(fileRequest.Address.Address))
                    manager = FileManagers[fileRequest.Address.Address];
            }

            // No Connection 
            if (manager == null)
            {
                if (fileRequest.ErrorCallback != null)
                    fileRequest.ErrorCallback(ErrorType.Connection);
                return;
            }

            manager.AnswerRequest(fileRequest);
        }


        #endregion



        #endregion


        #region Instance

        private readonly SafeCollection<string, FileRequest> _requestCollection = new SafeCollection<string, FileRequest>();
        private readonly SafeCollection<string, BaseFile> _collection = new SafeCollection<string, BaseFile>();
        private SystemFileIndexFile _hashIndexFile;
        private IPEndPoint Address { get; set; }
        private string _path;

        private FileManager(IPEndPoint address)
        {
            if (Me == null)
                throw new InvalidOperationException("must give FileManager static IP first");

            Address = address;
            CreateFolder();
           
            FileManagers[address.Address] = this;

            InitHashIndexFile();

            if(!Me.Equals(Address))
                RequestNewFile(HashIndexFileName);
        }

        private void InitHashIndexFile()
        {
            SystemFileIndexFile file = null;
            if(Exists(HashIndexFileName))
            {
                file = LoadFile(HashIndexFileName, typeof (SystemFileIndexFile)) as SystemFileIndexFile; 
            }
            else
            {
                file = new SystemFileIndexFile();
                file.Data = new byte[0];
                file.Name = HashIndexFileName;
                file.CloseAction = CloseAndSaveFile;
                _collection[file.Name] = file; 
            }

            if (file != null)
            {
                file.filesInformation.Clear();
                FileInfo[] files = new DirectoryInfo(_path).GetFiles();
                foreach (var f in files)
                {
                    string filename = f.Name;

                    if (filename != HashIndexFileName && f.Length <= (0x7f * FilePacketMaxSize))
                    {
                        var hash = GetHash(filename);
                        file.AddFileInfo(filename, hash);
                    }
                }
                _hashIndexFile = file;
                SafeFile(file.Name, file.Data, BitConverter.GetBytes(file.Hash));
            }

            if (!Me.Equals(Address))
            {
                _hashIndexFile.FileChanged += RequestNewFile;
            }

        }

        private void RequestNewFile(string name)
        {
            RequestFile(name,Address);
        }

        private void CreateFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(MainDirectory + "/" + Address.Address);
            if (!dir.Exists)
                dir.Create();
            _path = dir.FullName; 
        }

        public void Update(string name, byte[] data)
        {
            BaseFile item = null;
            lock (_collection)
            {
                item = _collection[name];
                if (item != null)
                {
                    item.Data = data;
                }
            }
            if (item == null)
            {
                SafeFile(name, data,  BitConverter.GetBytes(Crc32.CalculateHash(data)));
                CheckRequest(name, data);
            }
        }

        private void CheckRequest(string name, byte[] data)
        {
            var rq = _requestCollection[name];
            if (rq != null)
            {
                _requestCollection.Remove(name);
                AnswerRequest(rq); 
            }
        }

        private void AnswerRequest(FileRequest reqest)
        {
            if (Address.Equals(Me))
            {
                BaseFile file = Create(reqest.Name, reqest.type);
                if (file != null)
                {
                    reqest.File = file;
                    reqest.GotFileCallback();
                }
                else
                {
                    reqest.ErrorCallback(ErrorType.ResourceNotFound);
                }
            }
            else
            {
                BaseFile file = LoadFile(reqest.Name, reqest.type);
                if (file != null)
                {
                    reqest.File = file;
                    reqest.GotFileCallback();
                }
                else
                {
                    // Request Networkfile 
                    _requestCollection[reqest.Name] = reqest;
                }
            }
        }

        private void CloseFile(BaseFile file)
        {
            lock (_collection)
            {
                _collection.Remove(file.Name);
            }
        }

        private void CloseAndSaveFile(BaseFile file)
        {
            CloseFile(file); 
            SafeFile(file.Name, file.Data, BitConverter.GetBytes(file.Hash)); 

            _hashIndexFile.AddFileInfo(file.Name,file.Hash);
        }

        private void SafeFile(string name, byte[] data, byte[] hash = null)
        {
            int i = 0;
            while (i < 3)
            {
                try
                {
                    using (FileStream w = File.OpenWrite(_path + "/" + name))
                    {
                        w.Write(data, 0, data.Length);
                        if (hash != null)
                            w.Write(hash, 0, hash.Length);
                        w.Close();
                    }
                    break;
                }
                catch
                {
                    i++; 
                }
            }
        }

        private bool Exists(string name)
        {
            return File.Exists(_path + "/" + name);
        }

        private BaseFile LoadFile(string name, Type type)
        {
            lock (_collection)
            {
                if (_collection.ContainsFile(name))
                {
                    return _collection[name];
                }

                BaseFile file = null;
                FileInfo fileInfo = new FileInfo(_path + "/" + name);
                if (fileInfo.Exists)
                {
                    try
                    {
                        using (FileStream r = fileInfo.OpenRead())
                        {
                            long length = fileInfo.Length - Crc32.HashSize;
                            byte[] data = new byte[length];
                            byte[] hash = new byte[Crc32.HashSize];
                            r.Read(data, 0, (int) length);
                            r.Read(hash, 0, Crc32.HashSize);
                            r.Close();

                            file = Activator.CreateInstance(type) as BaseFile;
                            if (file != null)
                            {
                                file.Data = data;
                                file.Name = name;
                                file.Hash = BitConverter.ToUInt32(hash, 0);
                                if (Address.Equals(Me))
                                    file.CloseAction = CloseAndSaveFile;
                                else
                                    file.CloseAction = CloseFile;
                                _collection[file.Name] = file;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }


                return file;
            }
        }

        private UInt32 GetHash(string name)
        {
            lock (_collection)
            {
                if (_collection.ContainsFile(name))
                    return _collection[name].Hash;
            }

            FileInfo file = new FileInfo(_path + "/" + name);
            if (file.Exists)
            {
                using (FileStream r = file.OpenRead())
                {
                    r.Seek(file.Length - Crc32.HashSize, SeekOrigin.Begin);
                    byte[] hash = new byte[Crc32.HashSize];
                    r.Read(hash, 0, Crc32.HashSize);
                    r.Close();
                    return BitConverter.ToUInt32(hash, 0);
                }
            }
            return 0;
        }

        private byte[] GetFileData(string name)
        {
            lock (_collection)
            {
                if (_collection.ContainsFile(name))
                {
                    return _collection[name].Data;
                }

                byte[] data = null;
                FileInfo fileInfo = new FileInfo(_path + "/" + name);
                if (fileInfo.Exists)
                {
                    try
                    {
                        using (FileStream r = fileInfo.OpenRead())
                        {
                            long length = fileInfo.Length - Crc32.HashSize;
                            data = new byte[length];
                            r.Read(data, 0, (int)length);
                            r.Close();
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }


                return data;
            }
        }

        private BaseFile Create(string name, Type type)
        {
            if (Me.Equals(Address))
            {
                BaseFile info = LoadFile(name, type);
                if (info == null)
                {
                    lock (_collection)
                    {
                        info = Activator.CreateInstance(type) as BaseFile;
                        info.Data = new byte[0];
                        info.Hash = 0;
                        info.Name = name;
                        info.CloseAction = CloseAndSaveFile;
                        if (_collection.ContainsFile(name))
                        {
                            return _collection[name];
                        }
                    }
                }
                return info; 
            }

            throw new NotSupportedException("Only create on owner Machine");
        }

        #endregion 



        private class ReciveFile
        {
            public string Name { get; set; }

            public byte Session { get; set; }

            public byte Retransmit { get; set; }

            public int Size { get; private set; }

            public List<NetworkPacket> Packets = new List<NetworkPacket>();

            public TimeOut MaxWaitTimeout { get; set; }

            public void Add(NetworkPacket packet)
            {
                Size += packet.PayloadLength - 2;
                Packets.Add(packet);
            }

        }

    }
}
