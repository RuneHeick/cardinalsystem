using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkModules.Connection.Packet;

namespace NetworkModules.DataCentricSocket.Elements
{
    public class SocketHeader : IPacketElement
    {
        public SocketHeader()
        {
        }

        public byte Session
        {
            get { return Data[0]; }
            set
            {
                if (Data == null)
                    CreateData();
                Data[0] = value;
            }
        }

        private void CreateData()
        {
            Data = new byte[1];
        }


        public ICommandId Type { get; set; } // will be auto assigned; 

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
            get { return 1; }
        }

        public bool IsFixedSize
        {
            get { return true; }
        }
    }
}
