using Networking.Util;
using NetworkingLayer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Messageing
{
    public class CommandMessaging
    {
        private static TimeSpan CommandTimeOut = new TimeSpan(0,0,0,0,500);
        private UInt16 _commandIndex;
        private readonly object commandLock = new object();
        private Dictionary<UInt16, ReplyContainer> _commandSyncLib = new Dictionary<ushort, ReplyContainer>();


        public async Task<ComposedMessage> SendCommand(ComposedMessage msg, string address)
        {
            ReplyContainer item = new ReplyContainer();
            UInt16 key; 
            lock (commandLock)
            {
                unchecked
                {
                    _commandIndex++;
                }
                key = _commandIndex;
                _commandSyncLib.Add(key, item);
            }

            try
            {
                msg.AddToFront(BitConverter.GetBytes((UInt16)key));
                msg.AddToFront(new byte[] { (byte)MessageType.COMMAND_REQUEST });
                Task wait = item.awaiter.Wait().TimeoutAfter(CommandTimeOut);
                SendMessage?.Invoke(msg, address);
                await wait;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                lock (commandLock)
                {
                    _commandSyncLib.Remove(key);
                }
            }

            return item.reply;
        }

        public void ReplyCommand(Command cmd, ComposedMessage reply)
        {
            byte[] replyPart = new byte[1];
            replyPart[0] = (byte)MessageType.COMMAND_RESPONSE;
            byte[] id = cmd.CommandIDByte;
            reply.AddToFront(id);
            reply.AddToFront(replyPart);
            SendMessage?.Invoke(reply, cmd.Address);
        }



        public void HandleCommandReplys(byte[] packet, int startIndex, string from)
        {
            if (packet[startIndex] == (byte)MessageType.COMMAND_RESPONSE)
            {
                Command cmd = new Command(packet, startIndex, from, ReplyCommand);
                UInt16 key = cmd.CommandID;
                ReplyContainer container; 
   
                lock (commandLock)
                {
                    _commandSyncLib.TryGetValue(key, out container);
                }

                if (container != null)
                {
                    container.reply = new ComposedMessage();
                    container.reply.Add(packet);
                    container.awaiter.Pulse();
                }
            }
        }

        public void HandleCommandRequest(byte[] packet, int startIndex, string from)
        {
            if(packet[startIndex] == (byte)MessageType.COMMAND_REQUEST)
            {
                Command cmd = new Command(packet,startIndex, from, ReplyCommand);
                CommandRecived?.Invoke(cmd);
            }
        }

        public event Action<ComposedMessage, string> SendMessage;

        public event Action<Command> CommandRecived;
    }


    class ReplyContainer
    {
        public Awaiter awaiter = new Awaiter();
        public ComposedMessage reply = null;
    }
}
