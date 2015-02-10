using System;
using System.Text;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server3;
using Server3.Intercom.Network;
using Server3.Intercom.Network.Packets;

namespace ServerTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class NICTest
    {

        public NIC NetworkInterface1;
        public NIC NetworkInterface2; 

        public NICTest()
        {
            
        }


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            NetworkInterface1 = new NIC(new IPEndPoint(IPAddress.Loopback,5050));
            NetworkInterface2 = new NIC(new IPEndPoint(IPAddress.Loopback, 5051));
        }
        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            NetworkInterface1.Close();
            NetworkInterface2.Close();
        }
        //
        #endregion

        private bool done = false;
        private int retCount = 0;

        [TestMethod]
        public void UDPRequestTimeOut()
        {
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Udp, (o) => { }, (o, e) =>
            {
                done = true;
            }
            );
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            while (!done) ;
        }

        [TestMethod]
        public void TCPRequestTimeOut()
        {
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Tcp, (o) => { }, (o, e) =>
            {
                done = true;
            }
            );
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            while (!done) ;
        }


        [TestMethod]
        public void UDPRetransmitt()
        {
            EventBus.Subscribe<NetworkPacket>(UpdateRetCount);
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Udp, (o) => { }, (o, e) => { done = true; });
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            Thread.Sleep(700);

            Assert.IsTrue(retCount > 3);
        }

        [TestMethod]
        public void TcpRetransmitt()
        {
            EventBus.Subscribe<NetworkPacket>(UpdateRetCount);
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Tcp, (o) => { }, (o, e) => { done = true; });
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            Thread.Sleep(700);

            Assert.IsTrue(retCount == 1);
        }



        private void UpdateRetCount(NetworkPacket obj)
        {
            retCount++;
        }

        [TestMethod]
        public void TCPSignal()
        {
            EventBus.Subscribe<NetworkPacket>((o)=>GetPacket(o, PacketType.Tcp));
            NetworkPacket packet = NetworkRequest.CreateSignal(3, PacketType.Tcp);
            packet[0] = 2;
            packet.Address = NetworkInterface2.Ip; 
            NetworkInterface1.Send(packet);

            while (!done) ;

        }

        [TestMethod]
        public void UDPSignal()
        {
            EventBus.Subscribe<NetworkPacket>((o)=>GetPacket(o,PacketType.Udp));
            NetworkPacket packet = NetworkRequest.CreateSignal(3, PacketType.Udp);
            packet[0] = 2;
            packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(packet);

            while (!done) ;

        }

        [TestMethod]
        public void MulticastSignal()
        {
            EventBus.Subscribe<NetworkPacket>((o) => GetPacket(o, PacketType.Multicast));
            NetworkPacket packet = NetworkRequest.CreateSignal(3, PacketType.Multicast);
            packet[0] = 2;
            packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(packet);

            while (!done) ;

        }


        [TestMethod]
        public void TCPRequest()
        {
            EventBus.Subscribe<NetworkPacket>((o) => GotRequest(o, PacketType.Tcp));
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Tcp, (o) => { done = true; });
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            while (!done) ;

        }

        [TestMethod]
        public void UDPRequest()
        {
            EventBus.Subscribe<NetworkPacket>((o) => GotRequest(o, PacketType.Udp));
            NetworkRequest rq = NetworkRequest.Create(3, PacketType.Udp, (o) => { done = true; });
            rq.Packet[0] = 2;
            rq.Packet.Address = NetworkInterface2.Ip;
            NetworkInterface1.Send(rq);

            while (!done) ;
        }


        private void GotRequest(NetworkPacket o, PacketType packetType)
        {
            if (o.Type == packetType)
                o.SendReply(o);
        }


        private void GetPacket(NetworkPacket packet, PacketType type)
        {
            if(packet.Type == type)
                done = true; 
        }


        



    }
}
