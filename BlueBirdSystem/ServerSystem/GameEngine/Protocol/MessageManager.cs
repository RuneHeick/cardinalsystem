using Networking.Util;
using Sector;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Protocol
{

    public delegate void MessageHandler(Message message, string sender);

    public class MessageManager
    {

        public Dictionary<ProtocolCode, MessageHandler> _handlers = new Dictionary<ProtocolCode, MessageHandler>();


        public void Add(ProtocolCode type, MessageHandler message)
        {
            lock (_handlers)
                _handlers.Add(type, message);
        }





        public void network_dataReceived(IMsg msg)
        {
            
        }



    }
}
