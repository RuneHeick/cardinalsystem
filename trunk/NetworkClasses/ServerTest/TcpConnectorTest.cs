using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server3.Intercom.Network;
using Server3.Intercom.Network.NICHelpers;
using Server3.Intercom.Network.Packets;

namespace ServerTest
{
    [TestClass]
    public class TcpConnectorTest
    {
        public IPEndPoint _me;
        public TCPConnector _connector;
        public TCPConnector _connector2;

        public NetworkRequest _rq; 

        [TestInitialize]
        public void TestMethodSetup()
        {
            _me = new IPEndPoint(IPAddress.Loopback, 5050);
            _connector2 = new TCPConnector(_me);
            _connector = new TCPConnector(new IPEndPoint(IPAddress.Loopback, 5000));

            _rq = new NetworkRequest();
            _rq.Packet = new NetworkPacket(3);
            _rq.Packet[0] = 1;
            _rq.Packet[1] = 2;
            _rq.Packet[2] = 3;
            _rq.Packet.Address = _me;
        }

        [TestCleanup]
        public void TestMethodCleanup()
        {
            _connector.Stop();
            _connector2.Stop();
            _connector2 = null;
            _connector = null;
        }


        [TestMethod]
        public void Connect()
        {
            _connector.Send(_rq);
            Assert.AreEqual(_connector.OpenConnections, 1); 
            Thread.Sleep(40000);
            Assert.AreEqual(_connector.OpenConnections, 0); 
        }

        [TestMethod]
        public void ConnectToFail()
        {
            _rq.Packet.Address.Address = IPAddress.Parse("192.168.1.100");
            _connector.Send(_rq); 
            Thread.Sleep(40000);
            Assert.AreEqual(_connector.OpenConnections, 0);
        }

        [TestMethod]
        public void BundeldConnectionRq()
        {
            _connector.Send(_rq);
            _connector.Send(_rq);
            _connector.Send(_rq);
            Assert.AreEqual(_connector.OpenConnections, 1); 
        }

        bool done = false; 
        [TestMethod]
        public void RecivePacketSignal()
        {
            _connector2.OnPacketRecived += (o, s) => PacketRecived(o);
            _connector.Send(_rq);
            DateTime start = DateTime.Now; 
            while(done == false)
            {
                var time = DateTime.Now-start;
                if (time.TotalSeconds > 4)
                    Assert.Fail("No message Recived");
            }
        }

        [TestMethod]
        public void RecivePacketRq()
        {
            _connector2.OnPacketRecived += _connector2_OnPacketRecived;
            _rq.ResponseCallback = PacketRecived;
            _connector.Send(_rq);
            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 40)
                    Assert.Fail("No message Recived");
            }
        }

        void _connector2_OnPacketRecived(NetworkPacket arg1, IConnector arg2)
        {
            NetworkRequest rq = new NetworkRequest()
            {
                Packet = new NetworkPacket(1)
                {
                    Sesion = arg1.Sesion,
                    IsResponse = true,
                    Address = arg1.Address
                },
                ResponseCallback = null
            };
            _connector2.Send(rq);
        }

        private void PacketRecived(NetworkPacket obj)
        {
            done = true;
        }



    }
}
