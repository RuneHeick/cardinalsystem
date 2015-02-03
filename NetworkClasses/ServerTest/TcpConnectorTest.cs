using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server3.Intercom.Network;
using Server3.Intercom.Network.NICHelpers;

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
            _rq.Address = _me;
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
            _rq.Address.Address = IPAddress.Parse("192.168.1.100");
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

    }
}
