using Server.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.NewInterCom.Com
{
    class DataCollector
    {

        private Dictionary<IPAddress, DataStream> Collection = new Dictionary<IPAddress, DataStream>(new IPEqualityComparer()); 

        public void AddData(byte[] buffer, IPAddress sender)
        {
            lock (Collection)
            {
                if (Collection.ContainsKey(sender))
                {
                    Collection[sender].Add(buffer);
                }
                else
                {
                    DataStream stream = new DataStream(sender);
                    Collection.Add(sender, stream);
                    stream.OnPacketRecived += stream_OnPacketRecived;
                    stream.Add(buffer);
                }
            }
        }

        void stream_OnPacketRecived(DataStream stream, byte Command, byte[] packet)
        {
            lock (Collection)
            {
                if (stream.Count == 0)
                    Collection.Remove(stream.Address);

                Action<IPAddress, byte, byte[]> Event = OnPacketRecived; 
                if(Event != null)
                  Task.Factory.StartNew(()=>Event(stream.Address,Command,packet); 
            }
        }

        public event Action<IPAddress, byte, byte[]> OnPacketRecived; 
       
    }


    class DataStream
    {

        public int Count
        {
            get
            {
                return Stream.Count; 
            }
        }

        private List<byte> Stream = new List<byte>();

        public IPAddress Address { get; set; }

        public DataStream(IPAddress address)
        {
            Address = address; 
        }

        public void Add(byte[] data)
        {
            lock (Stream)
                Stream.AddRange(data);
            CheckForPacket();
        }

        public void Clear()
        {
            lock (Stream)
                Stream.Clear(); 
        }

        private void CheckForPacket()
        {
            if(Stream.Count>3)
            {
                int len = (Stream[1] << 8) + Stream[2];
                if(Stream.Count >= len+3)
                {
                    byte[] data;
                    byte command;

                    lock (Stream)
                    {
                        data = Stream.GetRange(3, len).ToArray();
                        command = Stream[0];
                        Stream.RemoveRange(0, len + 3);
                    }

                    Action<DataStream, byte, byte[]> Event = OnPacketRecived; 
                    if(Event != null)
                    {
                        Event(this, command, data); 
                    }

                    CheckForPacket();
                }
            }
        }

        public event Action<DataStream, byte, byte[]> OnPacketRecived; 


    }

}
