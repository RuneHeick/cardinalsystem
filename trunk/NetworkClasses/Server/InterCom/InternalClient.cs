using Server.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    class InternalClient
    {
        protected byte[] sizebuffer = new byte[1 + 4];
        protected byte[] Databuffer;
        int readindex = 0;

        TcpClient client;

        public IPAddress IP
        {
            get
            {
                return (client.Client.RemoteEndPoint as IPEndPoint).Address;
            }
        }

        public InternalClient(TcpClient client)
        {
            this.client = client;
            client.Client.BeginReceive(sizebuffer, 0, sizebuffer.Length, 0, DataReceivedSize, client.Client);
        }

        protected void DataReceivedSize(IAsyncResult result)
        {
            Socket stream = (Socket)result.AsyncState;
            SocketError Error;
            try
            {
                int len = stream.EndReceive(result, out Error);

                if (len != 0 && Error == SocketError.Success)
                {
                    readindex += len;

                    if (IsKnownMessage(sizebuffer[0]))
                    {
                        if (readindex == sizebuffer.Length)
                        {
                            int size = sizebuffer[1] + (sizebuffer[2] << 8) + (sizebuffer[3] << 16) + (sizebuffer[4] << 24);
                            Databuffer = new byte[size];
                            readindex = 0;
                            stream.BeginReceive(Databuffer, 0, Databuffer.Length, 0, DataReceivedData, stream);
                        }
                        else
                        {
                            stream.BeginReceive(sizebuffer, readindex, sizebuffer.Length - readindex, 0, DataReceivedSize, stream);
                        }
                    }
                    else // Will hopfully never be called
                    {
                        if (stream.Available > 0)
                        {
                            byte[] tempbuffet = new byte[stream.Available];
                            stream.Receive(tempbuffet);
                        }
                        stream.BeginReceive(sizebuffer, 0, sizebuffer.Length, 0, DataReceivedSize, stream);
                    }
                }
                else
                {
                    Disconnected();
                }
            }
            catch (Exception e)
            {
                Disconnected();
            }
        }

        private bool IsKnownMessage(byte Command)
        {
            return Enum.IsDefined(typeof(InternalNetworkCommands), (int)Command); ;
        }

        private void DataReceivedData(IAsyncResult result)
        {

            Socket stream = (Socket)result.AsyncState;
            SocketError Error;
            try
            {
                int len = stream.EndReceive(result, out Error);

                if (len != 0 && Error == SocketError.Success)
                {
                    readindex += len;
                    if (readindex == Databuffer.Length)
                    {
                        Action<InternalNetworkCommands, byte[], InternalClient> myEvent = OnDataRecived;
                        if (myEvent != null)
                            myEvent((InternalNetworkCommands)sizebuffer[0],Databuffer,this); 
                        readindex = 0;
                        stream.BeginReceive(sizebuffer, 0, sizebuffer.Length, 0, DataReceivedSize, stream);
                    }
                    else
                    {
                        stream.BeginReceive(Databuffer, readindex, Databuffer.Length - readindex, 0, DataReceivedData, stream);
                    }
                }
                else
                {
                    Disconnected();
                }

            }
            catch (Exception e)
            {
                Disconnected();
            }

        }

        public void Send(InternalNetworkCommands commands, byte[] data)
        {
            if (client.Connected == true)
            {
                if (data != null)
                {
                    try
                    {
                        byte[] startIndexer = new byte[] { (byte)commands, (byte)data.Length, (byte)((data.Length >> 8)), (byte)((data.Length >> 16)), (byte)((data.Length >> 24)) };
                        byte[] Sendbuffer = new byte[startIndexer.Length + data.Length];
                        Array.Copy(startIndexer, 0, Sendbuffer, 0, startIndexer.Length);
                        Array.Copy(data, 0, Sendbuffer, startIndexer.Length, data.Length);
                        NetworkStream stream = client.GetStream();
                        stream.WriteAsync(Sendbuffer, 0, Sendbuffer.Length, new System.Threading.CancellationToken());
                    }
                    catch
                    {
                        Disconnected();
                    }
                }
            }
        }

        private void Disconnected()
        {
            if (!isOnDisconnectCalled)
            {
                lock (client)
                {
                    if (!isOnDisconnectCalled)
                    {
                        isOnDisconnectCalled = true;
                        Action<InternalClient> myEvent = OnDisconnect;
                        if (myEvent != null)
                            myEvent(this); 
                    }
                }
            }
        }

        private bool isOnDisconnectCalled = false; 
        public event Action<InternalClient> OnDisconnect;
        public event Action<InternalNetworkCommands,byte[], InternalClient> OnDataRecived;

    }
}
