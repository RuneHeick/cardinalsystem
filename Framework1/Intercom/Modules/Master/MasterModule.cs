using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileModules;
using Intercom.Modules;
using Intercom.Protocols;
using Intercom.Protocols.Elements;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet.Commands;
using Utility;

namespace Intercom
{
    class MasterModule : INetworkModule
    {
        private readonly ConnectionManager _connectionsManager;
        private readonly SettingManager _networkSettings;
        private NetworkModule _networkModule;

        private WelcomeProtocol _protocol = new WelcomeProtocol(true);
        private readonly List<Connection> _connections = new List<Connection>();


        public MasterModule(ConnectionManager connections, SettingManager networkSettings, NetworkModule networkModule)
        {
            _connectionsManager = connections;
            _networkSettings = networkSettings;
            _networkModule = networkModule; 
            Initialization();
        }

        private void Initialization()
        {
            _connectionsManager.RemoteConnectionCreated += ConnectionEstablished; 
        }

        
        private void ConnectionEstablished(object sender, ConnectionEventArgs<Connection> e)
        {
            var connection = e.Connection;
            lock (_connections)
            {
                _connections.Add(connection);
                connection.Add(_protocol);
            }
        }




        public void Dispose()
        {
            _networkModule = null; 
        }
    }
}
