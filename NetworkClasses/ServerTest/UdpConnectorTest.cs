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
    public class UdpConnectorTest
    {
        public IPEndPoint _me;
        public UdpConnector _connector;
        public UdpConnector _connector2;

        bool done = false; 

        public NetworkRequest _rq; 

        [TestInitialize]
        public void TestMethodSetup()
        {
            _me = new IPEndPoint(IPAddress.Loopback, 5050);
            _connector2 = new UdpConnector(_me);
            _connector = new UdpConnector(new IPEndPoint(IPAddress.Loopback, 5000));

            _rq = CreateRQ();
        }

        private NetworkRequest CreateRQ(bool isSignal = false)
        {
            var rq = new NetworkRequest();
            rq.Packet = new NetworkPacket(3, PacketType.Udp, isSignal);
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
        public void RecivePacketSignal()
        {
            _connector2.OnPacketRecived += (o, s) => PacketRecived(o);
            _connector.Send(CreateRQ(true));
            DateTime start = DateTime.Now; 
            while(done == false)
            {
                var time = DateTime.Now-start;
                if (time.TotalSeconds > 5)
                    Assert.Fail("No message Recived");
            }
        }

        private int _recived = 0;
        private object Lock = new object(); 
        [TestMethod]
        public void RecivePacketSignalRuch()
        {
            int len = _rq.Packet.PacketLength;
            _connector2.OnPacketRecived += (o, s) => {
                                                         lock (Lock)
                                                         {
                                                             if(len == o.PacketLength)
                                                             _recived++;
                                                         } }; 
            
            for(int i = 0; i<100; i++)
                _connector.Send(_rq);

            Thread.Sleep(200);
            lock (Lock)
                Assert.IsTrue(100 == _recived);
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
                if (time.TotalSeconds > 400)
                    Assert.Fail("No message Recived");
            }
        }

        void _connector2_OnPacketRecived(NetworkPacket arg1, IConnector arg2)
        {
            arg1.SendReply(CreateRQ().Packet);
        }

        private void PacketRecived(NetworkPacket obj)
        {
            if (obj[0] == 1 && obj[1] == 2 && obj[2] == 3)
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

    }
}
