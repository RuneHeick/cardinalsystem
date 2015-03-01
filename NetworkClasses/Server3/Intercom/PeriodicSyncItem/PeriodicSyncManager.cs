using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server3.Intercom.Errors;
using Server3.Intercom.Network.Packets;
using Server3.Intercom.PeriodicSyncItem.File;
using Server3.Intercom.SharedFile;

namespace Server3.Intercom.PeriodicSyncItem
{
    internal class PeriodicSyncManager
    {
        public const string pInfofileName = "PeriodicTranslationTable";
        public static readonly TimeSpan PerioticMessageInterval = new TimeSpan(0,0,0,30);


        private readonly IPEndPoint _me;
        readonly Dictionary<IPAddress, PeriodicCollections> _endpoints = new Dictionary<IPAddress, PeriodicCollections>();
        private Timer _pSendTimer = null;  

        public PeriodicSyncManager(IPEndPoint me)
        {
            _me = me;
            EventBus.AddSubscribeNotifyer(typeof (FileRequest), Setup); 
        }

        private void Setup(Change change, Type type)
        {
            if(change != Change.Added)
                return;

            EventBus.RemoveSubscribeNotifyer(typeof(FileRequest), Setup);
            CreateNewEndPoint(_me);
            _pSendTimer = new Timer(SendPerioticMessage, null, PerioticMessageInterval, PerioticMessageInterval);
            EventBus.Subscribe<GetPeriodicMsgRequest>(GetPMessageRequest);
            EventBus.Subscribe<NetworkPacket>(PeriodicPacketRecived, (p) => p.Command == (byte)InterComCommands.PeriodicMsg);
        }

        private void GetPMessageRequest(GetPeriodicMsgRequest obj)
        {
            PeriodicMessage msg = null;
            lock (_endpoints)
            {
                if (_endpoints.ContainsKey(obj.Address))
                {
                    if(obj.Address.Equals(_me.Address))
                        msg = _endpoints[obj.Address].CreateOrGetMessage(obj.Name);
                    else
                        msg = _endpoints[obj.Address].GetMessage(obj.Name);
                }
            }

            if (msg != null && obj._callback != null)
            {
                obj._callback(msg);
            }
            else if (msg == null && obj._errorCallback != null)
            {
                obj._errorCallback(ErrorType.ResourceNotFound); 
            }
        }

        private void SendPerioticMessage(object state)
        {
            PeriodicCollections me = null;
            lock (_endpoints)
            {
                if (_endpoints.ContainsKey(_me.Address))
                    me = _endpoints[_me.Address];
            }
            if (me != null)
            {
                NetworkPacket message = me.GetPeriodicMsg();
                EventBus.Publich(message);
            }
        }

        private void PeriodicPacketRecived(NetworkPacket pMessage)
        {
            PeriodicCollections endpoint = null;
            lock (_endpoints)
            {
                if (!_endpoints.ContainsKey(pMessage.Address.Address))
                {
                    CreateNewEndPoint(pMessage.Address);
                    return;
                }
                else
                {
                    endpoint = _endpoints[pMessage.Address.Address];
                }
            }
            endpoint.Update(pMessage); 
        }

        private void CreateNewEndPoint(IPEndPoint iPEndPoint)
        {
            FileRequest pInfofile = FileRequest.CreateFileRequest<PeriodicTTFile>(pInfofileName, iPEndPoint, (o)=>GotpInfoFile(o,iPEndPoint.Address));
            EventBus.Publich(pInfofile);
        }

        private void GotpInfoFile(PeriodicTTFile file, IPAddress address)
        {
            lock (_endpoints)
            {
                if (!_endpoints.ContainsKey(address))
                {
                    PeriodicCollections collection = new PeriodicCollections(file, address);
                    _endpoints.Add(address,collection);
                }
            }
        }


    }
}
