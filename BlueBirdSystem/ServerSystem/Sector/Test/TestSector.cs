using System;
using System.Collections.Generic;
using System.Text;
using Networking.Util;

namespace Sector
{
    public class TestSector : ISector
    {
        public ulong Address { get; private set; }

        public event SectorStatusChanged OnSectorStatusChanged;
        public event SendMsgHandler OnSendMessage;


        public TestSector(ulong address)
        {
            Address = address;
        }


        public void MessageRecieved(IMsg msg)
        {
            

        }
        

        public void Tick()
        {
            OnSendMessage?.Invoke(this, new TestMsg(this));
        }

        public byte[] Save()
        {
            return new byte[100];
        }

        public void Load(byte[] data)
        {
            
        }

        public void ClientOffline(string clientAddress)
        {
            
        }
    }

    public class TestMsg : IClientMsg
    {
        private ComposedMessage msg = new ComposedMessage();
        private byte[] msgByte = new byte[100];

        public string Client { get; set; }

        public ulong ToSector { get; set; }

        public TestMsg(ISector sector)
        {
            Client = "test";
            ToSector = sector.Address;

            Random ran = new Random();
            ran.NextBytes(msgByte);
            msg.Add(msgByte);
        }


        public ComposedMessage GetMessage()
        {
            return msg;
        }
    }

}
