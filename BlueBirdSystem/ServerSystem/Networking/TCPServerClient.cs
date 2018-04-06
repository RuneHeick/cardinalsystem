using Networking;
using Networking.Util;
using NetworkingLayer.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetworkingLayer
{
    public class TCPServerClient: INetwork
    {
        private const int sizeFieldLength = 2;

        private string _address;
        private TcpClient _socket;
        private byte[] _data = new byte[sizeFieldLength];
        private int _currentReadIndex = 0;

        private readonly object _sendStateLock = new object(); 
        private SendState _sendState = SendState.Free;
        private Queue<ComposedMessage> _sendBuffer = new Queue<ComposedMessage>();

        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;

        public event SocketEventHandler OnSocketEvent;

        public TCPServerClient()
        {
            _socket = new TcpClient();
            _address = IPAddress.Loopback.ToString();
        }

        public TCPServerClient(TcpClient socket, string address)
        {
            _address = address;
            _socket = socket;
            StartRead();
        }

        //Client Only

        public void Connect(IPEndPoint ip)
        {
            _socket = new TcpClient();
            UpdateStatus(ConnectionState.Connecting, null);
            _socket.BeginConnect(ip.Address, ip.Port, AsyncConnectionComplete, null);
        }

        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                UpdateStatus(ConnectionState.Connected, null);
                StartRead();
            }
            catch (Exception e)
            {
                UpdateStatus(ConnectionState.Disconnected, null);
            }
        }


        //Getting Data 

        private void StartRead()
        {
            _data = new byte[sizeFieldLength];
            _socket.GetStream().BeginRead(_data, 0, sizeFieldLength, AsyncLengthReadComplete, null);
        }

        private void AsyncLengthReadComplete(IAsyncResult ar)
        {
            try
            {
                int len = _socket.GetStream().EndRead(ar);
                if (len == sizeFieldLength)
                {
                    int packetLen = BitConverter.ToUInt16(_data, 0);
                    _data = new byte[packetLen];
                    _socket.GetStream().BeginRead(_data, 0, packetLen, AsyncPacketReadComplete, null);
                }
                else
                {
                    UpdateStatus(ConnectionState.Disconnected, null);
                }
            }
            catch (Exception e)
            {
                UpdateStatus(ConnectionState.Disconnected, null);
            }
        }

        private void AsyncPacketReadComplete(IAsyncResult ar)
        {  
            try
            {
                int len = _socket.GetStream().EndRead(ar);
                _currentReadIndex += len;
                if (len > 0)
                {
                    if (_currentReadIndex == _data.Length)
                    { 
                        UpdateStatus(ConnectionState.Data, _data);
                        StartRead();
                    }
                    else
                    {
                        _socket.GetStream().BeginRead(_data, _currentReadIndex, _data.Length - _currentReadIndex, AsyncPacketReadComplete, null);
                    }
                }
                else
                {
                    UpdateStatus(ConnectionState.Disconnected, null);
                }
            }
            catch (Exception e)
            {
                UpdateStatus(ConnectionState.Disconnected, null);
            }
        }


        // Sending data
        public void Send(ComposedMessage data)
        {
            lock (_sendStateLock)
            {
                if (_sendState == SendState.Sending)
                    _sendBuffer.Enqueue(data);
                else
                {
                    byte[] length = BitConverter.GetBytes(((UInt16)data.Length));
                    _socket.GetStream().BeginWrite(length, 0, 2, AsyncWriteLengthSendComplete, new ComposedMessageIndexer(data));
                    _sendState = SendState.Sending;
                }
            }
        }

        private void AsyncWriteLengthSendComplete(IAsyncResult ar)
        {
            try
            {
                _socket.GetStream().EndWrite(ar);

                ComposedMessageIndexer indexer = (ComposedMessageIndexer)ar.AsyncState;
                byte[] data = indexer.Message.Message[indexer.Index];
                if (indexer.Message.Message.Count-1 > indexer.Index)
                {
                    indexer.Index++;
                    _socket.GetStream().BeginWrite(data, 0, data.Length, AsyncWriteLengthSendComplete, indexer);
                }
                else
                {
                    _socket.GetStream().BeginWrite(data, 0, data.Length, AsyncWriteComplete, indexer);
                }
            }
            catch (Exception e)
            {
                UpdateStatus(ConnectionState.Disconnected, null);
            }
        }

        private void AsyncWriteComplete(IAsyncResult ar)
        {
            try
            {
                _socket.GetStream().EndWrite(ar);
                lock (_sendStateLock)
                {
                    if(_sendBuffer.Count > 0)
                    {
                        ComposedMessage data = _sendBuffer.Dequeue();
                        byte[] length = BitConverter.GetBytes(((UInt16)data.Length));
                        _socket.GetStream().BeginWrite(length, 0, 2, AsyncWriteLengthSendComplete, new ComposedMessageIndexer(data));
                        _sendState = SendState.Sending;
                    }
                    else
                        _sendState = SendState.Free;
                }
            }
            catch (Exception e)
            {
                UpdateStatus(ConnectionState.Disconnected, null);
            }
        }


        // Notify Changes

        private void UpdateStatus(ConnectionState state, byte[] data)
        {
            ConnectionState oldState = State;
            if (state != ConnectionState.Data)
                State = state;

            if(oldState != state)
                OnSocketEvent?.Invoke(this, new SocketEvent() { State = state, Data = data, Address = _address });
        } 


    }
}
