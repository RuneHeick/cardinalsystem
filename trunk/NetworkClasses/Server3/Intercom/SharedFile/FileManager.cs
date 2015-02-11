using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Network;

namespace Server3.Intercom.SharedFile
{
    public class SharedFileManager
    {
        private readonly string _baseDirectory;
        private readonly IPAddress _me;
        private readonly Dictionary<IPAddress, LocalFileManager> _knownFileManagers = new Dictionary<IPAddress, LocalFileManager>();

        public SharedFileManager(string baseDirectory, IPAddress me  )
        {
            _baseDirectory = baseDirectory;
            _me = me;
            InitDirectory();

            EventBus.Subscribe<ClientFoundEvent>(NewClientFount);
        }

        #region Setup

        private void InitDirectory()
        {
            DirectoryInfo mainDir = new DirectoryInfo(_baseDirectory);
            if (mainDir.Exists)
            {
                DirectoryInfo[] dirs = mainDir.GetDirectories();
                foreach (var dir in dirs)
                {
                    string name = dir.Name;
                    IPAddress address = IPAddress.Parse(name);
                    LocalFileManager localFileManager = new LocalFileManager(address, dir);
                    lock (_knownFileManagers)
                    {
                        _knownFileManagers.Add(address, localFileManager);
                    }
                }
            }
            else
            {
                mainDir.Create();
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
                    LocalFileManager localFileManager = new LocalFileManager(client.Address.Address, folder);
                    _knownFileManagers.Add(client.Address.Address, localFileManager);
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


        #endregion

        #region FileUpdate



        #endregion
    }


    class LocalFileManager
    {
        private readonly IPAddress _address;
        private readonly DirectoryInfo _folder;
        private readonly Dictionary<string, BaseFile> _openFiles = new Dictionary<string, BaseFile>();

        public LocalFileManager(IPAddress address, DirectoryInfo folder)
        {
            _address = address;
            _folder = folder;
        }

        public T GetFile<T>(string name, bool create = false) where T : BaseFile, new()
        {
            lock (_openFiles)
            {
                if (_openFiles.ContainsKey(name))
                    return _openFiles[name] as T;

                T file = null;
                if (File.Exists(_folder.FullName + "/" + name) || create)
                    file = CreateOrOpenFile<T>(name);

                if(file != null)
                    _openFiles.Add(file.Name, file);
            }
            return null; 
        }

        private T CreateOrOpenFile<T>(string name) where T : BaseFile, new()
        {
            lock (_folder)
            {
                T file = null;
                try
                {
                    if (File.Exists(_folder.FullName + "/" + name))
                    {
                        byte[] data = File.ReadAllBytes(_folder.FullName + "/" + name);
                        file = new T()
                        {
                            Data = data,
                            Name = name,
                            CloseAction = SaveFile
                        };
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
                }
                catch
                {
                    // ignored
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
                    File.WriteAllBytes(file.Name, file.Data);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }
}
