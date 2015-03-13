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

        private readonly List<Connection> _connections = new List<Connection>();
        private readonly WelcomeProtocol _mainProtocol = new WelcomeProtocol(true);
        private readonly List<Protocol> _protocolsToAdd = new List<Protocol>(); 


        public MasterModule(ConnectionManager connections, SettingManager networkSettings, NetworkModule networkModule)
        {
            _connectionsManager = connections;
            _networkSettings = networkSettings;
            _networkModule = networkModule; 
            _protocolsToAdd.Add(_mainProtocol);
            Initialization();
        }

        public void AddProtocols(IEnumerable<Protocol> protocols)
        {
            lock(_protocolsToAdd)
                _protocolsToAdd.AddRange(protocols);
        }


        private void Initialization()
        {
            _connectionsManager.RemoteConnectionCreated += ConnectionEstablished;
            _mainProtocol.ClustorChanged += ClustorChanged;
        }

        private void ClustorChanged(object sender, ClustorEventArgs e)
        {
            
        }

        private void ConnectionEstablished(object sender, ConnectionEventArgs<Connection> e)
        {
            var connection = e.Connection;
            lock (_connections)
                _connections.Add(connection);
            lock (_protocolsToAdd)
                connection.AddRange(_protocolsToAdd);

            connection.OnStatusChanged+= ConnectionChanged;

        }

        private void ConnectionChanged(object sender, ConnectionEventArgs e)
        {
            if(e.Status == ConnectionStatus.Disconnected)
                Console.WriteLine("Lost connection to slave Sever");
        }


        public void Dispose()
        {
            lock (_connections)
                foreach (var connection in _connections)
                {
                    connection.RemoveRange(_protocolsToAdd);
                }
            _networkModule = null; 
        } 
    }
}
