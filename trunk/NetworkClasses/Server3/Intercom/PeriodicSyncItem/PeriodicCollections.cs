using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.Network;
using Server3.Intercom.Network.Packets;
using Server3.Intercom.PeriodicSyncItem.File;

namespace Server3.Intercom.PeriodicSyncItem
{
    class PeriodicCollections
    {
        private PeriodicTTFile _file;
        private IPAddress _address;
        private int _sendIndex = 0;
        private Dictionary<string, PeriodicMessage> _messages = new Dictionary<string,PeriodicMessage>(); 

        public PeriodicCollections(PeriodicTTFile file, IPAddress address)
        {
            _file = file;
            _address = address;
        }

        internal void Update(NetworkPacket pMessage)
        {

        }

        internal NetworkRequest GetPeriodicMsg()
        {
            List<byte> packet = new List<byte>();
            List<PeriodicMessage> messages;
            lock (_messages)
            {
                messages = _messages.Values.ToList();
            }
            bool isAdded = false; 
            int index = _sendIndex;
            while (packet.Count < 100 && packet.Count>0)
            {
                PeriodicMessage msg = messages[index];
                string name = msg.Name;
                byte[] data = msg.ToArray();
                byte length = (byte)msg.Count;

                byte? id = _file.GetId(name);
                if (!id.HasValue)
                {
                    _file.Add(name, length);
                    id = _file.GetId(name);
                    isAdded = true; 
                }

                packet.Add(id.Value);
                packet.AddRange(data);
                index = (index + 1) % messages.Count;

                if (index == _sendIndex)
                    break;
            }
            _sendIndex = index;

            var rq = NetworkRequest.CreateSignal(packet.Count, PacketType.Multicast);
            NetworkPacket.Copy(rq.Packet, 0, packet, 0, packet.Count);
            rq.Packet.Command = (byte)InterComCommands.PeriodicMsg;

            
            if(isAdded)
                _file.Save();

            return rq;
        }

        internal PeriodicMessage GetMessage(string name)
        {
            lock (_messages)
            {
                if (_messages.ContainsKey(name))
                    return _messages[name];
            }
            return null; 
        }

        internal PeriodicMessage CreateOrGetMessage(string name)
        {
            lock (_messages)
            {
                PeriodicMessage msg = GetMessage(name);
                if (msg == null)
                {
                    msg = new PeriodicMessage(name);
                    _messages.Add(name, msg);
                }
                return msg;
            }
        }
    }
}
