using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server3.Intercom.SharedFile
{
    public class BaseFile:IDisposable
    {
        private readonly object _objectLock = new object();
        internal Action<BaseFile> CloseAction;
        
        public virtual byte[] Data { get; set; }
        public string Name { get; internal set; }

        public byte[] Hash { get; internal set;  }

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
