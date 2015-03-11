using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class NetworkModule
    {
        private readonly SettingManager _networkSettings = new SettingManager("Network Settings.txt");
        private readonly ConnectionManager _connections;
        private readonly IPEndPoint _localEndPoint;

        private INetworkModule _module;

        public NetworkModule(IPEndPoint localEndPoint)
        {
            _localEndPoint = localEndPoint;
            _connections = new ConnectionManager(localEndPoint);
            Initialization();
        }

        private void Initialization()
        {
            var master = _networkSettings.GetOrCreateSetting("Master", "Network Master", _localEndPoint.ToString());
            _currentMaster = master.Value.CreateIpEndPoint();

            if (_currentMaster.Equals(_localEndPoint))
                InitMaster();
            else
                InitSlave();
        }

        private void InitSlave()
        {
            if (_module != null)
                _module.Dispose();
            _module = new SlaveModule(_connections, this);
        }

        private void InitMaster()
        {
            if (_module != null)
                _module.Dispose();
            _module = new MasterModule(_connections, this);
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
