using Messageing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class Commands
    {

        [TestMethod]
        public void SendCommand()
        {
            byte[] ReplyData = new byte[300];
            Random ran = new Random();
            ran.NextBytes(ReplyData);

            byte[] RequestData = new byte[300];
            ran.NextBytes(RequestData);

            ComposedMessage relpyMsg = new ComposedMessage();
            relpyMsg.Add(ReplyData);

            ComposedMessage requestMsg = new ComposedMessage();
            requestMsg.Add(RequestData);

            CommandMessaging messaging = new CommandMessaging(); 
            messaging.CommandRecived += (cmd)=>
            {
                byte[] rec = cmd.Data;
                byte[] rec2 = new byte[rec.Length - 3];
                Array.Copy(rec, 3, rec2, 0, rec2.Length);
                CollectionAssert.AreEqual(RequestData, rec2);

                cmd.Reply(relpyMsg);
            };

            messaging.SendMessage += (msg, addrass) =>
            {
                byte[] data = fromComposedMessage(msg);

                if (data[0] == (byte)MessageType.COMMAND_REQUEST)
                    messaging.HandleCommandRequest(data, 0, "");
                else if (data[0] == (byte)MessageType.COMMAND_RESPONSE)
                    messaging.HandleCommandReplys(data, 0, "");
            };

            Task<ComposedMessage> recieved = messaging.SendCommand(requestMsg, "");
            recieved.Wait();

            if (recieved.Result == null)
                Assert.Fail();
            else
            {
                byte[] rec = fromComposedMessage(recieved.Result);
                byte[] rec2 = new byte[rec.Length - 3];
                Array.Copy(rec, 3, rec2, 0, rec2.Length);
                CollectionAssert.AreEqual(ReplyData, rec2);
            }

        }


        [TestMethod]
        public void SendCommandTimeout()
        {
            byte[] ReplyData = new byte[300];
            Random ran = new Random();
            ran.NextBytes(ReplyData);

            byte[] RequestData = new byte[300];
            ran.NextBytes(RequestData);

            ComposedMessage relpyMsg = new ComposedMessage();
            relpyMsg.Add(ReplyData);

            ComposedMessage requestMsg = new ComposedMessage();
            requestMsg.Add(RequestData);

            CommandMessaging messaging = new CommandMessaging();
            messaging.CommandRecived += (cmd) =>
            {
                byte[] rec = cmd.Data;
                byte[] rec2 = new byte[rec.Length - 3];
                Array.Copy(rec, 3, rec2, 0, rec2.Length);
                CollectionAssert.AreEqual(RequestData, rec2);
                Thread.Sleep(510);
                cmd.Reply(relpyMsg);
            };

            messaging.SendMessage += (msg, addrass) =>
            {
                byte[] data = fromComposedMessage(msg);

                if (data[0] == (byte)MessageType.COMMAND_REQUEST)
                    messaging.HandleCommandRequest(data, 0, "");
                else if (data[0] == (byte)MessageType.COMMAND_RESPONSE)
                    messaging.HandleCommandReplys(data, 0, "");
            };

            try
            {
                Task<ComposedMessage> recieved = messaging.SendCommand(requestMsg, "");
                recieved.Wait();

                if (!(recieved.IsFaulted && recieved.Exception.GetType().Equals(typeof(TimeoutException))))
                    Assert.Fail();
            }
            catch(TimeoutException e)
            {

            }
            catch(Exception e)
            {
                if(!(e.InnerException != null && e.InnerException.GetType().Equals(typeof(TimeoutException))))
                    Assert.Fail();
            }
            
        }



        private static byte[] fromComposedMessage(ComposedMessage msg)
        {
            byte[] data = new byte[msg.Length];
            int length = 0;
            for (int i = 0; i < msg.Message.Count; i++)
            {
                Array.Copy(msg.Message[i], 0, data, length, msg.Message[i].Length);
                length += msg.Message[i].Length;
            }
            return data;
        }
    }
}
