using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Errors;

namespace Server3.Intercom.SharedFile
{
    internal class FileRequest
    {
        public static FileRequest CreateFileRequest<T>(string name, IPEndPoint address, Action<T> gotFileCallback, Action<ErrorType> errorCallback) where T: BaseFile , new ()
        {
            var rq = new FileRequest(name, address, errorCallback);
            rq.GotFileCallback = () => { gotFileCallback((T) rq.File); };
            rq.type = typeof (T);

            return rq;
        }

        private FileRequest(string name, IPEndPoint address,Action<ErrorType> errorCallback)
        {
            Name = name;
            Address = address;
            ErrorCallback = errorCallback;
        }

        internal Type type { get; set; }

        internal BaseFile File { get; set; }

        public string Name { get; private set; }

        public IPEndPoint Address { get; private set; }

        internal Action GotFileCallback { get; private set; }

        internal Action<ErrorType> ErrorCallback { get; private set; }
    }
}
