using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Errors;

namespace Server3.Intercom.PeriodicSyncItem
{
    internal class GetPeriodicMsgRequest
    {
        internal readonly Action<PeriodicMessage> _callback;
        internal readonly Action<ErrorType> _errorCallback;
        public string Name { get; private set; }
        public IPAddress Address { get; private set; }


        public GetPeriodicMsgRequest(string name, IPAddress address, Action<PeriodicMessage> callback, Action<ErrorType> errorCallback = null)
        {
            _callback = callback;
            _errorCallback = errorCallback;
            Name = name;
            Address = address;
        }

       
    }
}
