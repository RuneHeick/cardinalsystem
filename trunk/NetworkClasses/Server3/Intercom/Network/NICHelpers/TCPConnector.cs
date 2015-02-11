﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Server.InterCom;
using Server.Utility;
using Server3.Intercom.Network.Packets;

namespace Server3.Intercom.Network.NICHelpers
{
    public class TCPConnector : IConnector
    {
        private const int ConnectionMaxIdleTime_ms = 30000;

        readonly List<ClientInfo> _connectedClients = new List<ClientInfo>();

        private readonly TcpListener _listener;

        public IPEndPoint Address { get; private set; }

        public int OpenConnections
        {
            get { return _connectedClients.Count; }
        }

        public TCPConnector(IPEndPoint ip)
        {
            Address = ip; 
            _listener = new TcpListener(ip.Address,ip.Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        public void Close()
        {
            _listener.Stop();
            lock (_connectedClients)
            {
                foreach (var client in _connectedClients)
                {
                    client.Client.Close();
                }
            }
        }

        private void AsyncAcceptClientComplete(IAsyncResult ar)
        {
            try
            {
                var socket = _listener.EndAcceptTcpClient(ar);
                var info = AddClient(socket, ((IPEndPoint) socket.Client.RemoteEndPoint).Address, false);
                StartRead(info);
            }
            catch (Exception)
            {

                // ignored
            }
            _listener.BeginAcceptTcpClient(AsyncAcceptClientComplete, null);
        }

        public void Send(NetworkPacket packet)
        {
            lock (_connectedClients)
            {
                var connection = _connectedClients.FirstOrDefault(
                    (o) => o.Address.Equals(packet.Address.Address));
                if (connection == null)
                    StartConnect(packet);
                else if (connection.Client.Connected && connection.IsConnecting)
                {
                    connection.ToSend.Add(packet);
                }
                else if (connection.Client.Connected && connection.IsConnecting == false)
                    StartSendReqest(connection, packet);
                
            }
        }

        private void StartConnect(NetworkPacket packet)
        {
            lock (_connectedClients)
            {
                var connection = _connectedClients.FirstOrDefault(
                   (o) => o.Address.Equals(packet.Address.Address));
                if (connection == null)
                {
                    ClientInfo info = AddClient(new TcpClient(), packet.Address.Address, true);
                    info.ToSend.Add(packet);
                    info.Client.BeginConnect(packet.Address.Address, packet.Address.Port, AsyncConnectionComplete, info);
                }
            }
        }

        private void AsyncConnectionComplete(IAsyncResult ar)
        {
            lock (_connectedClients)
            {
                ClientInfo info = (ClientInfo) ar.AsyncState;
                try
                {
                    info.Client.EndConnect(ar);
                    info.IsConnecting = false;
                    StartRead(info);
                }
                catch (Exception e)
                {
                    CloseClientInfo(info);
                }
            
                foreach (var packet in info.ToSend)
                {
                    StartSendReqest(info,packet);
                }
                info.ToSend.Clear();
            }
        }

        private void StartRead(ClientInfo info)
        {
            info.Client.GetStream()
                    .BeginRead(info.InfoBuffer, 0, info.InfoBuffer.Length, AsyncInfoReadComplete, info);
        }

        private void AsyncInfoReadComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                int len = info.Client.GetStream().EndRead(ar);
                if (len == info.InfoBuffer.Length)
                {
                    int packetLen = NetworkPacket.GetPacketLength(info.InfoBuffer);
                    info.PacketBuffer = new byte[packetLen];
                    info.Client.GetStream().BeginRead(info.PacketBuffer, 0, packetLen, AsyncPacketReadComplete, info);
                }
                else
                {
                    CloseClientInfo(info);
                }
            }
            catch (Exception e)
            {
                CloseClientInfo(info);
            }
        }

        private void AsyncPacketReadComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                int len = info.Client.GetStream().EndRead(ar);
                if (len == info.PacketBuffer.Length)
                {
                    var infoBuffer = info.InfoBuffer;
                    var packetBuffer = info.PacketBuffer;
                    var RecivedTime = DateTime.Now;

                    info.LastTouch = RecivedTime;
                    info.BufferReset();

                    StartRead(info); //Start Reading before using thread to handle Packet. 

                    NetworkPacket recivedPacket = new NetworkPacket(infoBuffer, packetBuffer, this, PacketType.Tcp)
                    {
                        TimeStamp = RecivedTime, 
                        Address = info.Client.Client.RemoteEndPoint as IPEndPoint
                    };

                    PacketRecived(recivedPacket);
                }
                else
                {
                    CloseClientInfo(info);
                }
            }
            catch (Exception)
            {
                CloseClientInfo(info);
            }
        }

        private void PacketRecived(NetworkPacket packet)
        {
            try
            {
                    Action<NetworkPacket, IConnector> Event = OnPacketRecived;
                    if (Event != null)
                        Event(packet, this);
            }
            catch
            {
                // Packet handle error 
            }
        }

        private void StartSendReqest(ClientInfo stream, NetworkPacket networkPacket)
        {
            byte[] packet = networkPacket.Packet;
            stream.Client.GetStream()
                .BeginWrite(packet, 0, packet.Length, AsyncWriteComplete, stream);
        }

        private void AsyncWriteComplete(IAsyncResult ar)
        {
            ClientInfo info = (ClientInfo) ar.AsyncState;
            try
            {
                info.Client.GetStream().EndWrite(ar);
                info.LastTouch = DateTime.Now;
            }
            catch (Exception)
            {
                CloseClientInfo(info);
            }
        }

        private void CloseClientInfo(ClientInfo info)
        {
            lock (_connectedClients)
            {
                _connectedClients.Remove(info);
                info.CloseTimer.Calcel();

                if (info.Client != null)
                {
                    info.Client.Close();
                }
            }
        }

        private ClientInfo AddClient(TcpClient socket, IPAddress address, bool isConnecting)
        {
            ClientInfo info = new ClientInfo()
            {
                Address = address,
                Client = socket,
                LastTouch = DateTime.Now,
                IsConnecting = isConnecting,
            };
            info.CloseTimer = TimeOut.Create(30000, info, CloseClientInfo);
            lock (_connectedClients)
                _connectedClients.Add(info);

            return info;
        }


        public PacketType Supported
        {
            get { return PacketType.Tcp; }
        }

        public event Action<NetworkPacket, IConnector> OnPacketRecived;


        private class ClientInfo
        {
            public ClientInfo()
            {
                ToSend = new List<NetworkPacket>();
                BufferReset();
            }

            public IPAddress Address { get; set; }

            public byte[] InfoBuffer { get; set; }

            public byte[] PacketBuffer { get; set; }

            public bool IsConnecting { get; set; }

            public TcpClient Client { get; set; }

            private DateTime _lastTouch;

            public TimeOut CloseTimer { get; set; }

            public DateTime LastTouch
            {
                get { return _lastTouch; }
                set
                {
                    _lastTouch = value;
                    if(CloseTimer != null)
                        CloseTimer.Touch();
                }
            }

            public void BufferReset()
            {
                InfoBuffer = new byte[3];
                PacketBuffer = null; 
            }

            public List<NetworkPacket> ToSend { get; set; } 
        }

    }
}