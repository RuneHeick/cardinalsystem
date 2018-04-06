using Networking.Util;
using NetworkingLayer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Networking
{
    public class UDPServerClient : INetwork
    {
        public IPEndPoint _address { get; private set; }
        readonly UdpClient _listener;

        public event SocketEventHandler OnSocketEvent;

        public UDPServerClient(IPEndPoint address)
        {
            _address = address;
            _listener = new UdpClient(_address);
            StartRecive();
        }

        public UDPServerClient()
        {
            _listener = new UdpClient();
        }


        public void Close()
        {
            try
            {
                _listener.Close();
            }
            catch (Exception)
            {
                //ignore 
            }
        }


        private void StartRecive()
        {
            _listener.BeginReceive(AsyncUdpReciveComplete, null);
        }

        private void AsyncUdpReciveComplete(IAsyncResult ar)
        {
            try
            {
                IPEndPoint from = new IPEndPoint(0, 0);
                byte[] data = _listener.EndReceive(ar, ref from);

                PacketRecived(from, data);

                StartRecive();
            }
            catch (ObjectDisposedException e)
            { }
            catch (Exception e)
            {
                StartRecive();
            }
        }

        public void Send(ComposedMessage packet, IPEndPoint to)
        {
            byte[] data;
            List<byte[]> content = packet.Message;
            if (content.Count == 1)
                data = content[0];
            else
            {
                data = new byte[packet.Length];
                int index = 0; 
                for(int i = 0; i< content.Count; i++)
                {
                    byte[] part = content[i];
                    Array.Copy(part, 0, data, index, part.Length);
                    index += part.Length;
                }
            }

            _listener.BeginSend(data, data.Length, to, AsyncCallbackComplete, null);
        }

        private void AsyncCallbackComplete(IAsyncResult ar)
        {
            try
            {
                _listener.EndSend(ar);
            }
            catch
            {
                // ignored
            }
        }


        private void PacketRecived(IPEndPoint from, byte[] data)
        {
            OnSocketEvent?.Invoke(this, new NetworkingLayer.Util.SocketEvent() { Address = from.ToString(), State = ConnectionState.Data, Data = data });
        }

    }
}
