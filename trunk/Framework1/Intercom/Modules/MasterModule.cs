using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileModules;
using Intercom.Modules;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet.Commands;
using Utility;

namespace Intercom
{
    class MasterModule : INetworkModule
    {
        private readonly ConnectionManager _connections;
        private NetworkModule _networkModule;

        public MasterModule(ConnectionManager connections, NetworkModule networkModule)
        {
            _connections = connections;
            _networkModule = networkModule; 
            Initialization();
        }

        private void Initialization()
        {
            CommandCollection commandCollection = CommandCollection.Instance;
            commandCollection.ResetCommands();
            //commandCollection.GetOrCreateCommand<>(); Create base protocoles for operation

            commandCollection.CreateProtocolDefinition();
            _connections.RemoteConnectionCreated += ConnectionEstablished; 
        }

        
        private void ConnectionEstablished(object sender, ConnectionEventArgs<Connection> e)
        {
             // Add protocoles 

        }




        public void Dispose()
        {
            _networkModule = null; 
        }
    }
}
