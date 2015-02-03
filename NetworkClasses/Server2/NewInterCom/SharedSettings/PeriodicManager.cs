using Server.InterCom;
using Server.NewInterCom.Com;
using Server.NewInterCom.SharedSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Server.Utility;
using Server2.Utility;

namespace Server.NewInterCom
{
    public class PeriodicManager
    {
        private const int PeriodicInterval = 5000; 
        const int MaxPacketLength = 100;
        private const string TranslationFileName = "PeriodicManagerTranslation"; 

        private readonly Dictionary<IPAddress, List<PeriodicMessage>> _messages;
        private int _messagesIndex = 0;
        private readonly Dictionary<IPAddress, StringToByteDir> _translations;
        private readonly ComLink _comLink;


        private readonly EventManager<PeriodicMessage, IPAddress> _periodicMessageAddedEventManager;
        private readonly Action<PeriodicMessage, IPAddress> _publishFunction; 
        public EventManager<PeriodicMessage, IPAddress> PeriodicMessageAddedEventManager
        {
            get { return _periodicMessageAddedEventManager; }
        }

        public PeriodicManager(ComLink link)
        {
            var cmp = new IPEqualityComparer();
            _messages = new Dictionary<IPAddress, List<PeriodicMessage>>(cmp);
            _translations = new Dictionary<IPAddress, StringToByteDir>(cmp);

            _messages.Add(link.Address.Address,new List<PeriodicMessage>());

            _comLink = link;
            SetupPeriodicMessageTranslation();
            var pitem = GetMessage("Port", link.Address.Address); 
            pitem.SetValue(link.Address.Port);

            EventManagerSetupInfo<PeriodicMessage, IPAddress> info = new EventManagerSetupInfo<PeriodicMessage, IPAddress>();
            _periodicMessageAddedEventManager = new EventManager<PeriodicMessage, IPAddress>(info);
            _publishFunction = info.Publish;

            SettingManager.Subscribe<StringToByteDir>(FileChanged, TranslationFileName); 

            SetupNetwork();

        }

        private void FileChanged(StringToByteDir file, SettingManager manager)
        {
            lock (_translations)
            {
                if (_translations.ContainsKey(manager.Address))
                    _translations[manager.Address] = file;
                else
                    _translations.Add(manager.Address, file);
            }
        }

        private async void SetupPeriodicMessageTranslation()
        {
            var settingManager = SettingManager.GetSettingManager(_comLink.Address.Address);
            string name = TranslationFileName; 
            if (!settingManager.HasSetting(name))
            {
                var translationDir = await settingManager.CreateSetting<StringToByteDir>(name);
                translationDir.Add("Port");
                lock (_translations)
                    _translations.Add(_comLink.Address.Address, translationDir);
            }
            else
            {
                var translationDir = await settingManager.GetSetting<StringToByteDir>(name);
                lock (_translations)
                    _translations.Add(_comLink.Address.Address, translationDir);
            }
        }

        public PeriodicMessage GetMessage(string name, IPAddress address)
        {
            lock (_messages)
            {
                if (_messages.ContainsKey(address))
                {
                    var message = _messages[address].FirstOrDefault((o) => o.Name == name);
                    if (message != null)
                        return message;

                    //Not in messages
                    if (address.Equals(_comLink.Address.Address))
                    {
                        message = new PeriodicMessage(name);
                        _messages[address].Add(message);
                        _translations[_comLink.Address.Address].Add(name);
                    }

                }
            }
            return null;
        }

        public PeriodicMessage GetMessage(string name)
        {
            return GetMessage(name, _comLink.Address.Address); 
        }

        public void RemoveMessage(string name)
        {
            lock (_messages)
            {
                _messages[_comLink.Address.Address].RemoveAll((o) => o.Name == name); 
            }
        }

        private byte[] GetPeriodicMessage()
        {
            lock (_messages)
            {
                var messages = _messages[_comLink.Address.Address];
                if (messages.Count > 0)
                {
                    List<byte> message = new List<byte>(MaxPacketLength);
                    int startIndex = _messagesIndex;
                    do
                    {
                        var msg = messages[_messagesIndex];
                        var msgdata = msg.ToArray();
                        byte id = TranslateString(msg.Name, _comLink.Address.Address);
                        if (id != 255)
                        {
                            message.Add(id);
                            message.Add((byte) msgdata.Length);
                            message.AddRange(msgdata);
                        }

                        _messagesIndex = _messagesIndex + 1%_messages.Count;
                    } while (_messagesIndex != startIndex && message.Count < MaxPacketLength);

                    return message.ToArray();
                }
            }
            return null; 
        }

        public string TranslateByte(byte id, IPAddress sender)
        {
            if (id == 0) return "Port";
            lock (_translations)
            {
                if (_translations.ContainsKey(sender))
                {
                    var table = _translations[sender];
                    return table[id];
                }
            }
            return "";
        }

        public byte TranslateString(string id, IPAddress sender)
        {
            if (id == "Port") return 0;
            lock (_translations)
            {
                if (_translations.ContainsKey(sender))
                {
                    var table = _translations[sender];
                    return table[id];
                }
            }
            return 255;
        }

        #region Network 

        private void SetupNetwork()
        {
            _comLink.PacketRecived += PacketRecived;
            Timer sendTimer = new Timer(SendTimerCallBack, null, PeriodicInterval, PeriodicInterval);
        }

        private void SendTimerCallBack(object state)
        {
            byte[] packet = GetPeriodicMessage();
            _comLink.Send(InternalNetworkCommands.Periodic, packet, null, SendType.Multicast); 
        }

        private void PacketRecived(InternalNetworkCommands command, byte[] packet, IPAddress address, SendType type)
        {
            if (type == SendType.Multicast && command == InternalNetworkCommands.Periodic)
            {
                if (_translations.ContainsKey(address))
                {
                    for (int i = 0; i < packet.Length; i++)
                    {
                        byte id = packet[i];
                        byte len = packet[i + 1]; 
                        byte[] data = new byte[len];
                        Array.Copy(packet, i + 2, data,0,len);

                        var messageName = TranslateByte(id, address);
                        if (messageName != "")
                        {
                            var msg = GetMessage(messageName, address);
                            if (msg != null)
                            {
                                msg.SetValue(data); 
                            }
                            else
                            {
                                AddNewMessage(messageName, data, address);
                            }
                        }

                        i += len + 1; 
                    }


                }
            }
        }

        private void AddNewMessage(string messageName, byte[] data, IPAddress address)
        {
            if(!_messages.ContainsKey(address))
                _messages.Add(address,new List<PeriodicMessage>());
            var msg = new PeriodicMessage(messageName);
            msg.SetValue(data);
            _messages[address].Add(msg);
            if (messageName == "Port" && !_comLink.IsAccepted(address))
            {
                _comLink.AddAcceptedAddress(address, msg.ToInt());
            }
            _publishFunction(msg, address); 
        }

 
        #endregion 

        // must be created from factory 
        public class PeriodicMessage
        {

            public PeriodicMessage(string name)
            {
                Name = name; 
            }

            public string Name { get; private set; }

            byte[] info; 
            byte[] value_
            {
                set
                {
                    if(value != null)
                    {
                        if(value.Length>MaxPacketLength)
                            throw new FormatException(" Infomation is to long");
                        info = value; 
                    }
                }
                get
                {
                    return info; 
                }
            }

            public void SetValue(string Value)
            {
                    value_ = UTF8Encoding.UTF8.GetBytes(Value);
            }

            public void SetValue(int Value)
            {
                try
                {
                    value_ = BitConverter.GetBytes(Value);
                }
                catch
                {
                    value_ = null;
                }
            }

            public void SetValue(long Value)
            {
                try
                {
                    value_ = BitConverter.GetBytes(Value);
                }
                catch
                {
                    value_ = null;
                }
            }

            public void SetValue(byte Value)
            {
                try
                {
                    value_ = new byte[]{Value};
                }
                catch
                {
                    value_ = null;
                }
            }

            public void SetValue(byte[] Value)
            {
                value_ = Value; 
            }

            public override string ToString()
            {
                return UTF8Encoding.UTF8.GetString(value_);
            }

            public int ToInt()
            {
                if (value_ == null)
                    return 0;
                try
                {
                    return BitConverter.ToInt32(value_,0);
                }
                catch
                {
                    return 0;
                }
            }

            public long ToLong()
            {
                if (value_ == null)
                    return 0;
                try
                {
                    return BitConverter.ToInt64(value_,0);
                }
                catch
                {
                    return 0;
                }
            }

            public byte ToByte()
            {
                if (value_ == null)
                    return 0;
                try
                {
                    return value_[0]; 
                }
                catch
                {
                    return 0;
                }
            }

            public byte[] ToArray()
            {
                if (value_ == null)
                    return null;
                try
                {
                    return value_;
                }
                catch
                {
                    return null;
                }
            }
        }

    }


    public enum MsgType : byte
    {

    }
    
}
