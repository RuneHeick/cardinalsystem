using Server.NewInterCom.SharedSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.NewInterCom
{
    class SharedSettingsManager
    {
        const int MaxPacketLength = 100; 
        private List<PeriodicMessage> Messages = new List<PeriodicMessage>();
        private int MessagesIndex = 0; 
        private StringToByteDir PeriodicMessageTranslation; 


        public SharedSettingsManager()
        {
            
        }

        public PeriodicMessage GetMessage(string Name)
        {
            lock (Messages)
            {
                var message = Messages.FirstOrDefault((o) => o.Name == Name);
                if (message == null)
                {
                    message = new PeriodicMessage(Name);
                    Messages.Add(message);
                    PeriodicMessageTranslation.Add(Name);
                }

                return message;
            }
        }

        public void RemoveMessage(string Name)
        {
            lock(Messages)
            {
                Messages.RemoveAll((o) => o.Name == Name); 
            }
        }

        private byte[] GetPeriodicMessage()
        {
            if(Messages.Count>0)
            {
                List<byte> message = new List<byte>(20);
                message.Add((byte)MsgType.Periodic); 
                int startIndex = MessagesIndex;
                do
                {
                    var msg = Messages[MessagesIndex];
                    var msgdata = msg.ToArray();
                    byte id = PeriodicMessageTranslation[msg.Name]; 
                    if(id != 255)
                    {
                        message.Add(id);
                        message.Add((byte)msgdata.Length);
                        message.AddRange(msgdata);
                    }

                    MessagesIndex = MessagesIndex + 1 % Messages.Count; 
                }
                while (MessagesIndex != startIndex && message.Count < MaxPacketLength);

                return message.ToArray(); 
            }
            return null; 
        }
       

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
        Periodic,
        FileTransfer,
        ConnectionInfo
    }
    
}
