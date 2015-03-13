using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Intercom.Protocols.Elements;
using NetworkModules.Connection;
using NetworkModules.Connection.Connector;
using NetworkModules.Connection.Packet;
using NetworkModules.Connection.Packet.Commands;

namespace Intercom.Protocols
{
    class WelcomeProtocol:Protocol
    {
        private readonly ClusterElement _cluster = new ClusterElement();
        private readonly Dictionary<Type, Action<PacketElement, Connection>> _handels = new Dictionary<Type, Action<PacketElement, Connection>>();
        private bool _isMaster;

        public WelcomeProtocol(bool isMaster)
        {
            IsMaster = isMaster; 
        }

        public override void HandlePacket(Connection from, NetworkModules.Connection.Packet.NetworkPacket networkPacket)
        {
            var baseElement = networkPacket.Elements[0];
            var type = baseElement.GetType();
            if (_handels.ContainsKey(type))
                _handels[type](baseElement, from); 
        }

        public bool IsMaster
        {
            get { return _isMaster; }
            set
            {
                if (_isMaster != value)
                {
                    _isMaster = value;
                    _handels.Clear();
                    PacketDefinitions.Clear();
                    if (_isMaster)
                    {
                        PacketDefinition hello = new PacketDefinition(new List<Type>() {typeof (HelloElement)});
                        PacketDefinitions.Add(hello);
                        _handels.Add(typeof (HelloElement), (e, c) => HandleHello((HelloElement) e, c));
                    }
                    else
                    {
                        PacketDefinition clustor = new PacketDefinition(new List<Type>() {typeof (ClusterElement)});
                        PacketDefinition command = new PacketDefinition(new List<Type>() {typeof (PCommandElement)});
                        PacketDefinition redirect = new PacketDefinition(new List<Type>() { typeof(MasterRedirectElement) });
                        PacketDefinition hello = new PacketDefinition(new List<Type>() { typeof(HelloElement) });
                        PacketDefinitions.Add(clustor);
                        PacketDefinitions.Add(command);
                        PacketDefinitions.Add(redirect);
                        PacketDefinitions.Add(hello);
                        _handels.Add(typeof (ClusterElement), (e, c) => HandleCluster((ClusterElement) e, c));
                        _handels.Add(typeof (PCommandElement), (e, c) => HandlePCommandElement((PCommandElement) e, c));
                        _handels.Add(typeof(MasterRedirectElement), (e, c) => HandleRedirectElement((MasterRedirectElement)e, c));
                        _handels.Add(typeof(HelloElement), (e, c) => HandleWrongHello((HelloElement)e, c));
                    }
                }
            }
        }

        

        private object HandleRedirectElement(MasterRedirectElement masterRedirectElement, Connection c)
        {
            throw new NotImplementedException();
        }

        private void HandleHello(HelloElement element, Connection from)
        {
            lock(_cluster)
                _cluster.Add(from.RemoteEndPoint, element.PerformanceIndex);
            SendClustorInfo(from);
            SendProtocol(from);
            OnClustorChanged(_cluster);
        }

        private object HandleWrongHello(HelloElement helloElement, Connection c)
        {
            throw new NotImplementedException();
        }

        private void HandleCluster(ClusterElement element, Connection from)
        {
            lock (_cluster)
                _cluster.Data = element.Data;
            OnClustorChanged(_cluster);
        }

        private void HandlePCommandElement(PCommandElement element, Connection from)
        {
            CommandCollection collection = CommandCollection.Instance;
            collection.CheckCollection(element.Data);
        }

        public void SendHello(Connection connection)
        {
            NetworkPacket packet = new NetworkPacket();
            packet.Type = PacketType.Tcp;

            int index = SystemTester.GetStatus();
            HelloElement element = new HelloElement() { PerformanceIndex = index };
            packet.Add(element);
            connection.Send(packet);
        }

        private void SendClustorInfo(Connection connection)
        {
            lock (_cluster)
            {
                NetworkPacket packet = new NetworkPacket() {Type = PacketType.Tcp};
                packet.Elements.Add(_cluster);
                connection.Send(packet);
            }
        }

        private void SendProtocol(Connection connection)
        {
            CommandCollection collection = CommandCollection.Instance;
            var col = collection.GetCollection();
            NetworkPacket packet = new NetworkPacket(){Type = PacketType.Tcp};
            packet.Elements.Add(new PCommandElement(){Data = col});
            connection.Send(packet);
        }

        internal List<ClusterElement.ClientInfo> GetClientInfos
        {
            get { return _cluster.Clients; }
        }

        public event ClusterChangedHandler ClustorChanged;

        protected virtual void OnClustorChanged(ClusterElement element)
        {
            var handler = ClustorChanged;
            if (handler != null)
            {
                ClustorEventArgs e = new ClustorEventArgs(element);
                handler(this, e);
            }
        }
    }
}
