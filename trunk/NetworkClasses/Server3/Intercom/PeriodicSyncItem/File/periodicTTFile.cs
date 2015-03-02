using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Server3.Intercom.SharedFile;

namespace Server3.Intercom.PeriodicSyncItem.File
{
    internal class PeriodicTTFile:BaseFile
    {
        private readonly Dictionary<byte, DirItem> _translationTable = new Dictionary<byte, DirItem>();


        public override byte[] Data
        {
            get
            {
                byte[] data = null;

                lock (_translationTable)
                {
                    int size = _translationTable.Values.Sum((o) => o.Name.Length + 2);
                    data = new byte[size];
                    int index = 0;
                    foreach (var translation in _translationTable.Values)
                    {
                        data[index] = translation.Size;
                        Encoding.UTF8.GetBytes(translation.Name, 0, translation.Name.Length, data, index + 1);
                        data[index + translation.Name.Length + 1] = 0;
                        index = index + translation.Name.Length + 2;
                    }
                }
                return data;
            }
            set
            {
                if(value == null) return;
                
                byte[] data = value;
                int startID = 0;
                byte command = 0; 
                while (true)
                {
                    byte length = data[startID];
                    int t = Array.FindIndex(data, startID + 1, (o) => o == (byte)0);
                    string name = UTF8Encoding.UTF8.GetString(data, startID + 1, t - startID-1);
                    Add(command, name, length);
                    if (data.Length == t + 1)
                        break;
                    startID = t + 1;
                    command++;
                }
                
            }
        }

        public void Add(byte command, string name, byte length)
        {
            if (!_translationTable.ContainsKey(command))
            {
                _translationTable.Add(command,new DirItem(){Name = name, Size = length});
            }
        }


        private class DirItem
        {
            public string Name;
            public byte Size; 
        }
    }
}
