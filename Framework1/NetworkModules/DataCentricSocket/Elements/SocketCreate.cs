using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace NetworkModules.DataCentricSocket.Elements
{
    class SocketCreate : IPacketElement
    {

        public ICommandId Type
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public byte[] Data
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Length
        {
            get { return 4;  }
        }

        public bool IsFixedSize
        {
            get { return true;  }
        }
    }
}
