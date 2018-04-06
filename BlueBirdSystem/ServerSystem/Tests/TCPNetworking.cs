using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class TCPNetworking
    {

        [TestMethod]
        public void ConnectionTest()
        {
            NetworkingLayer.TCPServer server = null;
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, 3030);
                server = new NetworkingLayer.TCPServer(ip);


                NetworkingLayer.TCPServerClient client = new NetworkingLayer.TCPServerClient();
                client.Connect(ip);

                while(client.State == NetworkingLayer.ConnectionState.Connecting) { Thread.Sleep(5); };
                if (client.State != NetworkingLayer.ConnectionState.Connected)
                    Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
            finally
            {
                if (server != null)
                    server.StopServer();
            }
        }

        [TestMethod]
        public void ConnectionTestNoServer()
        {
            try
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, 3030);

                NetworkingLayer.TCPServerClient client = new NetworkingLayer.TCPServerClient();
                client.Connect(ip);
                Assert.Fail();
            }
            catch (Exception e)
            {

            }
        }


        [TestMethod]
        public void ConnectionAndSendData()
        {
            NetworkingLayer.TCPServer server = null;
            try
            {
                byte[] data = new byte[300];
                Random ran = new Random(2000);
                ran.NextBytes(data);

                //Start Server
                IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, 4040);
                server = new NetworkingLayer.TCPServer(ip);

                //Start Client
                NetworkingLayer.TCPServerClient client = new NetworkingLayer.TCPServerClient();
                client.Connect(ip);

                //Wait for connection
                while (client.State == NetworkingLayer.ConnectionState.Connecting) { Thread.Sleep(5); };
                if (client.State != NetworkingLayer.ConnectionState.Connected)
                    Assert.Fail();

                byte[] RecievedData = null;
                server.OnSocketEvent += (s, arg) =>
                {
                    if (arg.State != NetworkingLayer.ConnectionState.Data)
                        Assert.Fail();
                    if (arg.State == NetworkingLayer.ConnectionState.Data)
                        RecievedData = arg.Data;
                };
                var msg = new Networking.Util.ComposedMessage();
                msg.Add(data);
                msg.Add(data);
                client.Send(msg);

                while (RecievedData == null) { Thread.Sleep(5); };

                byte[] validate = new byte[2 * data.Length];
                Array.Copy(data, validate, data.Length);
                Array.Copy(data, 0, validate, data.Length, data.Length);

                CollectionAssert.AreEqual(validate, RecievedData);
            }
            catch (Exception e)
            {
                Assert.Fail();
            }
            finally
            {
                if (server != null)
                    server.StopServer();
            }
        }
    }
}
