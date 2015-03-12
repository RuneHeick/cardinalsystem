using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intercom.Modules;
using NetworkModules.Connection;

namespace Intercom
{
    class SlaveModule : INetworkModule
    {
        private ConnectionManager _connections;
        private NetworkModule _networkModule;

        public SlaveModule(ConnectionManager connections,Connection masterConnection ,NetworkModule networkModule)
        {
            _networkModule = networkModule; 
            _connections = connections;
        }



        public void Dispose()
        {
            _networkModule = null; 
        }
    }
}
