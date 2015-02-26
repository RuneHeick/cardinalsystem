using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Server.Utility;
using Server3.Intercom.Errors;
using Server3.Intercom.SharedFile;
using Server3.Intercom.SharedFile.Files;
using Server3.Utility;

namespace Server3.Intercom.SharedFile2
{
    internal class FileManager
    {
        #region Static

        public const string MainDirectory = "DataManager";
        private const string HashIndexFileName = "FileInfo";
        private const int FilePacketMaxSize = 1000; 

        private static readonly SafeCollection<IPAddress, FileManager> FileManagers = new SafeCollection<IPAddress, FileManager>(new IPEqualityComparer());

        public static IPAddress Me { get; set; }

        static FileManager()
        {
            Me = null; 
            DirectoryInfo dir = new DirectoryInfo(MainDirectory);
            if (!dir.Exists)
                dir.Create();
        }



        #endregion


        #region Instance

        private readonly SafeCollection<string, FileRequest> _requestCollection = new SafeCollection<string, FileRequest>();
        private readonly SafeCollection<string, BaseFile> _collection = new SafeCollection<string, BaseFile>();
        private SystemFileIndexFile _hashIndexFile;
        private IPAddress Address { get; set; }
        private string _path; 

        public FileManager(IPAddress address)
        {
            if (Me == null)
                throw new InvalidOperationException("must give FileManager static IP first");

            Address = address;
            CreateFolder();
           
            FileManagers[address] = this;

            InitHashIndexFile();
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
            }

            if (file != null)
            {
                file.filesInformation.Clear();
                FileInfo[] files = new DirectoryInfo(_path).GetFiles();
                foreach (var f in files)
                {
                    if (f.Name != HashIndexFileName && f.Length <= (0x7f * FilePacketMaxSize))
                    {
                        var hash = GetHash(file.Name);
                        file.AddFileInfo(file.Name, hash);
                    }
                }
                _hashIndexFile = file;
                _hashIndexFile.Address = Address; 
            }

        }

        private void CreateFolder()
        {
            DirectoryInfo dir = new DirectoryInfo(MainDirectory + "/" + Address);
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
                SafeFile(name, data);
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
                        r.Read(data, 0, (int)length);
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
    }
}
