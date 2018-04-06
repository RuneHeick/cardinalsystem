using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Messageing
{
    public class Command
    {
        const int COMMAND_TYPE_INDEX = 0;
        const int COMMAND_ID_INDEX = 1;
        const int COMMAND_DATA_INDEX = 3;

        private byte[] data;
        private int startIndex;
        private Action<Command, ComposedMessage> replyCommand;
        private string address;

        public UInt16 CommandID
        {
            get
            {
                return BitConverter.ToUInt16(data, startIndex + COMMAND_ID_INDEX);
            }
        }

        public byte[] CommandIDByte
        {
            get
            {
                return new byte[] { data[startIndex + COMMAND_ID_INDEX], data[startIndex + COMMAND_ID_INDEX + 1] };
            }   
        }

        public int StartIndex { get { return startIndex; } }

        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        public string Address
        {
            get
            {
                return address;
            }
        }

        public Command(byte[] packet, int startIndex, string address, Action<Command,ComposedMessage> replyCommand)
        {
            this.data = packet;
            this.startIndex = startIndex;
            this.replyCommand = replyCommand;
            this.address = address;
        }


        public void Reply(ComposedMessage msg)
        {
            replyCommand(this, msg);
        }



    }
}
