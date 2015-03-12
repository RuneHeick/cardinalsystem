using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FileModules;
using Intercom.Modules;
using Intercom.Protocols.Elements;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet.Commands;
using Utility;

namespace Intercom
{
    public class NetworkModule
    {
        private readonly SettingManager _networkSettings;
        private readonly ConnectionManager _connections;
        private readonly IPEndPoint _localEndPoint;

        private INetworkModule _module;

        public NetworkModule(string settingPath)
        {
            _networkSettings = new SettingManager(settingPath);
            var localEndPoint = _networkSettings.GetOrCreateSetting("LocalEndPoint", "Local Information", new IPEndPoint(IPAddress.Loopback,9000).ToString());
            _localEndPoint = localEndPoint.Value.CreateIpEndPoint();
            _connections = new ConnectionManager(_localEndPoint);

            CommandCollection commandCollection = CommandCollection.Instance;
            commandCollection.ResetCommands();
            commandCollection.GetOrCreateCommand<HelloElement>();
            commandCollection.GetOrCreateCommand<ClusterElement>();
            commandCollection.GetOrCreateCommand<PCommandElement>();
            commandCollection.CreateProtocolDefinition();

            Initialization();
        }

        private void Initialization()
        {
            var master = _networkSettings.GetOrCreateSetting("Master", "Network Master", _localEndPoint.ToString());
            var currentMaster = master.Value.CreateIpEndPoint();

            if (currentMaster.Equals(_localEndPoint))
                InitMaster();
            else
                InitSlave();
        }

        private void InitSlave()
        {
            if (_module != null)
                _module.Dispose();
            _module = new SlaveModule(_connections,null ,this);
        }

        private void InitMaster()
        {
            if (_module != null)
                _module.Dispose();
            _module = new MasterModule(_connections, _networkSettings, this);
        }

        internal void SwitchToSlave()
        {
            InitSlave();
        }

        internal void SwitchToMaster()
        {
            InitMaster();
        }


    }
}
