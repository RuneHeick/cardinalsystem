using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server.NewInterCom.SharedSettings;
using Server.NewInterCom;
using Server.NewInterCom.Com;

namespace Server2.NewInterCom
{
    public class InterCom
    {
        private ComLink _comLink; 

        public IPEndPoint Me { get; private set; }
        public PeriodicManager PeriodicCom { get; set; }

        public InterCom(IPEndPoint me)
        {
            Me = me;
            _comLink = new ComLink(Me);
            SettingManager.CreateManager(Me.Address);
            PeriodicCom = new PeriodicManager(_comLink); 
        }
    }

}
