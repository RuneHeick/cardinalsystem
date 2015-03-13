using System;
using Intercom.Protocols.Elements;
using NetworkModules.Connection.Helpers;
using NetworkModules.Connection.Packet;

namespace NetworkModules.Connection.Connector
{

    public delegate void ClusterChangedHandler(object sender, ClustorEventArgs e);


    public class ClustorEventArgs : EventArgs
    {
        public ClustorEventArgs(ClusterElement clustor)
        {
            Clustor = clustor; 
        }

        public ClusterElement Clustor { get; private set; }
    }

    


}
