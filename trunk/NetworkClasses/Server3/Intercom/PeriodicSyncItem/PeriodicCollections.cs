using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Network.Packets;
using Server3.Intercom.PeriodicSyncItem.File;

namespace Server3.Intercom.PeriodicSyncItem
{
    class PeriodicCollections
    {
        private PeriodicTTFile _file;
        private IPAddress _address;

        public PeriodicCollections(PeriodicTTFile file, IPAddress address)
        {
            _file = file;
            _address = address;
        }

        internal void Update(NetworkPacket pMessage)
        {
            
        }

        internal NetworkPacket GetPeriodicMsg()
        {

            return null;
        }

        internal PeriodicMessage GetMessage(string p)
        {
            throw new NotImplementedException();
        }

        internal PeriodicMessage CreateOrGetMessage(string p)
        {
            throw new NotImplementedException();
        }
    }
}
