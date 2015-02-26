using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server3.Utility;

namespace Server3.Intercom.SharedFile
{
    public class BaseFile:IDisposable
    {
        private readonly object _objectLock = new object();
        internal Action<BaseFile> CloseAction;
        private byte[] _data;
        private uint _hash;

        public virtual byte[] Data
        {
            get
            {
                return _data;
            }
            set { _data = value; }
        }

        protected virtual void UpdateHash()
        {
            Hash = Crc32.CalculateHash(Data);
        }

        public string Name { get; internal set; }

        public UInt32 Hash
        {
            get
            {
                UpdateHash();
                return _hash;
            }
            internal set { _hash = value; }
        }

        public void Dispose()
        {
            lock (_objectLock)
            {
                if (CloseAction != null)
                {
                    CloseAction(this);
                    CloseAction = null;
                }
            }
        }
    }
}
