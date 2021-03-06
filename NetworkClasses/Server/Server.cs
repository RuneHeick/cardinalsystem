﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.NetworkInformation;
using Server.InterCom;
using System.Net;
using System.Net.Sockets;
using Server.Client; 

namespace Server
{
    public class Server
    {
        private ServerCom InterCom;

        private Dictionary<IPAddress, IClient> Clients = new Dictionary<IPAddress, IClient>(); 

        private UdpClient UDPRequest; 
        private TcpListener TcpListenSocket;
        private IPEndPoint Me;

        public Server(int Port)
        {
            if (!isPortFree(Port))
            {
                Port = 0; // Random assign port; 
            }
            var ip = GetLocalIP();
            Me = new IPEndPoint(IPAddress.Parse(ip), Port);
            UDPRequest = new UdpClient(Me); 
            TcpListenSocket = new TcpListener(Me);
            TcpListenSocket.Start();
            TcpListenSocket.BeginAcceptTcpClient(ClientJoind, TcpListenSocket);
            InterCom = new ServerCom(Me);

            UDPRequest.BeginReceive(UDP_RequestRecived, UDPRequest);

        }

        public void Test()
        {

            InterCom.Send(IPAddress.Parse("192.168.87.101"), new byte[] { 0, 1, 2, 3, 4, 5 }); 

        }


        private void UDP_RequestRecived(IAsyncResult ar)
        {
            try
            {
                UDPRequest.BeginReceive(UDP_RequestRecived, UDPRequest);
                IPEndPoint remoteEndpoint = new IPEndPoint(Me.Address, 0);
                byte[] data = UDPRequest.EndReceive(ar, ref remoteEndpoint);

            }
            catch
            {

            }

        }

        private void ClientJoind(IAsyncResult ar)
        {
            TcpClient client = null;
            try
            {
                TcpListenSocket.BeginAcceptTcpClient(ClientJoind, TcpListenSocket);


                client = TcpListenSocket.EndAcceptTcpClient(ar);
                var ip = client.Client.RemoteEndPoint as IPEndPoint;
                if (ip != null)
                {
                   
                }
                client.Close();
            }
            catch
            {
                if (client != null)
                    client.Close();
            }

        }

        private bool isPortFree(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable; 
        }

        private string GetLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }


    }
}
