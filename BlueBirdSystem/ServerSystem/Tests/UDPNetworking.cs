using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking;
using System;
using System.Net;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class UDPNetworking
    {

        [TestMethod]
        public void UDPConnectionTest()
        {
            UDPServerClient client = new UDPServerClient(new IPEndPoint(IPAddress.Any, 9090));
            client.Close();
        }

        [TestMethod]
        public void UDPConnectionSendData()
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 9090);
            UDPServerClient client = new UDPServerClient(ip);

            byte[] rdata = null;
            client.OnSocketEvent += (s, arg) =>
            {
                rdata = arg.Data;
            };

            UDPServerClient client2 = new UDPServerClient();
            byte[] data = new byte[300];
            Random ran = new Random(2000);
            ran.NextBytes(data);

            var msg = new Networking.Util.ComposedMessage();
            msg.Add(data);
            msg.Add(data);
            client2.Send(msg, new IPEndPoint(IPAddress.Loopback, ip.Port));

            byte[] validate = new byte[2 * data.Length];
            Array.Copy(data, validate, data.Length);
            Array.Copy(data, 0, validate, data.Length, data.Length);
         

            while(rdata == null) { Thread.Sleep(5); }

            CollectionAssert.AreEqual(validate, rdata);
        }

    }
}
