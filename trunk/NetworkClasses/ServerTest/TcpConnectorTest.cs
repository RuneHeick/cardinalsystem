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

        bool done = false; 

        public NetworkRequest _rq; 

        [TestInitialize]
        public void TestMethodSetup()
        {
            _me = new IPEndPoint(IPAddress.Loopback, 5050);
            _connector2 = new TCPConnector(_me);
            _connector = new TCPConnector(new IPEndPoint(IPAddress.Loopback, 5000));

            _rq = CreateRQ();
        }

        private NetworkRequest CreateRQ()
        {
            var rq = new NetworkRequest();
            rq.Packet = new NetworkPacket(3,PacketType.Tcp);
            rq.Packet[0] = 1;
            rq.Packet[1] = 2;
            rq.Packet[2] = 3;
            rq.Packet.Address = _me;


            return rq;
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
        }

        [TestMethod]
        public void CloseInactiveConnection()
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
            arg1.SendReply(CreateRQ());
        }

        private void PacketRecived(NetworkPacket obj)
        {
            done = true;
        }

        [TestMethod]
        public void RequestTimeout()
        {
            _rq.ResponseCallback = (o)=>{};
            _rq.ErrorCallbak = (o,s) =>
            { if (s == ErrorType.TimeOut) done = true; };
            _connector.Send(_rq);
            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 6)
                    Assert.Fail("Timeout to slow");
            }
        }


        [TestMethod]
        public void RequestTimeoutCancel()
        {
            _connector2.OnPacketRecived += _connector2_OnPacketRecived;
            _rq.ResponseCallback = (o) => { };
            _rq.ErrorCallbak = (o, s) =>
            { done = true; };
            _connector.Send(_rq);
            Thread.Sleep(6000);
            Assert.IsFalse(done);
        }

        [TestMethod]
        public void RequestTimeoutDisconnect()
        {
            _rq.ResponseCallback = (o) => { };
            _rq.ErrorCallbak = (o, s) =>
            { if(s==ErrorType.Connection) done = true; };
            _connector.Send(_rq);
            _connector.Stop();
            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 6)
                    Assert.Fail("Timeout to slow");
            }
        }

        [TestMethod]
        public void RequestOwerflow()
        {
            
            for (int i = 0; i < 130; i++)
            {
                var rq = CreateRQ();
                rq.ResponseCallback = (o) => { };
                rq.ErrorCallbak = (o, s) =>
                { if (s == ErrorType.RequestFull) done = true; };
                _connector.Send(rq);
            }

            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 6)
                    Assert.Fail("Timeout to slow");
            }
        }

        [TestMethod]
        public void ReplySignalError()
        {
            _connector2.OnPacketRecived += (o, s) =>
            {
                try
                {
                    o.SendReply(CreateRQ());
                }
                catch (InvalidOperationException)
                {
                    done = true;
                }
            };
            _connector.Send(_rq);
            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 4)
                    Assert.Fail("No message Recived");
            }
        }

        private int _recived = 0;
        private object Lock = new object();
        [TestMethod]
        public void RecivePacketSignalRuch()
        {
            int len = _rq.Packet.PacketLength;
            _connector2.OnPacketRecived += (o, s) =>
            {
                lock (Lock)
                {
                    if (len == o.PacketLength)
                        _recived++;
                    else
                        Assert.Fail("Packet Error");
                }
            };

            for (int i = 0; i < 100; i++)
                _connector.Send(CreateRQ());
            Thread.Sleep(200);
            lock (Lock)
                Assert.IsTrue(100 == _recived);
        }


        [TestMethod]
        public void CommandSignal()
        {
            _connector2.OnPacketRecived += (o, s) => { if (o.Command == 5) done = true; };
            _rq.Packet.Command = 5; 
            _connector.Send(_rq);
            DateTime start = DateTime.Now;
            while (done == false)
            {
                var time = DateTime.Now - start;
                if (time.TotalSeconds > 4)
                    Assert.Fail("No message Recived");
            }
        }

        [TestMethod]
        public void CommandRq()
        {
            _connector2.OnPacketRecived += (o, s) => { if (o.Command == 5) done = true; }; ;
            _rq.Packet.Command = 5; 
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

        [TestMethod]
        public void CommandToBig()
        {
            try
            {
                _rq.Packet.Command = 32;
                Assert.Fail("To big Command");
            }
            catch (ArgumentOutOfRangeException)
            {
                
            }
            
        }


    }
}
