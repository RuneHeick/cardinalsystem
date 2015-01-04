using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.InterCom
{
    
    public enum InterComCommands
    {
        Who = 0, // Asks who is on the network 
        IAm = 1, // Gives personal stats
    }


    public class WhoCom:IAmCom
    {
        public override InterComCommands ID
        {
            get
            {
                return InterComCommands.Who;
            }
        }

        public override byte[] Command
        {
            get
            {
                byte[] data = base.Command;
                data[0] = (byte)ID;
                return data; 
            }
        }
    }

    public class IAmCom
    {
        public string IP {get; set;}

        public int Port {get; set;}

        public ushort NetState {get; set;}

        public virtual InterComCommands  ID
        {
            get
            {
                return InterComCommands.IAm;
            }
        }

        public virtual byte[] Command
        {
            get
            {
                byte[] ip = IPAddress.Parse(IP).GetAddressBytes();
                byte[] port = BitConverter.GetBytes(Port);
                byte[] netState = BitConverter.GetBytes(NetState);
                byte[] data = new byte[ip.Length + port.Length + netState.Length + 1];
                Array.Copy(ip, 0, data, 1, ip.Length);
                Array.Copy(port, 0, data, ip.Length+1, port.Length);
                Array.Copy(netState, 0, data, port.Length + ip.Length + 1, netState.Length);
                data[0] = (byte)ID;
                return data; 
            }
            set
            {
                IP = value[1].ToString() + "." + value[2].ToString() + "." + value[3].ToString() + "." + value[4].ToString();
                Port = BitConverter.ToInt32(value, 5);
                NetState = BitConverter.ToUInt16(value, 9); 
            }
        }
        

    }


}
