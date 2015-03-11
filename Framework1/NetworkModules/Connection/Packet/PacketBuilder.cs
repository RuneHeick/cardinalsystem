using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkModules.Connection.Packet.Commands;

namespace NetworkModules.Connection.Packet
{
    class PacketBuilder
    {
        public const int MaxPacketSize = 0x0000ffff; 

        private readonly List<PacketElement> _elements = new List<PacketElement>();

        private readonly object _syncRoot = new object();
        private byte[] _packetContainer = null;

        public ReadOnlyCollection<PacketElement> Elements
        {
            get { return _elements.AsReadOnly(); }
        }

        public void Add(PacketElement element)
        {
            lock (_syncRoot)
            {
                if (!element.ExpectedSize.Equals(Size.Dynamic))
                    _elements.Insert(0, element);
                else
                {
                    if (_elements.Count == 0 || _elements[_elements.Count - 1].ExpectedSize != Size.Dynamic)
                        _elements.Add(element);
                    else
                        throw new InvalidOperationException("Only one dynamic packet can be in an element collection");
                }
            }
        }

        public void Remove(PacketElement element)
        {
            lock (_syncRoot)
            {
                _elements.Remove(element);
            }
        }

        public byte[] CreatePackets()
        {

            lock (_syncRoot)
            {
                if (_elements.Count == 0)
                    throw new InvalidOperationException("No Packet data is added");

                int dataLength = _elements.Sum((o) => o.Length);
                var elementsCount = _elements.Count;
                var packetLength = elementsCount + dataLength;

                if (packetLength > MaxPacketSize)
                    throw new IndexOutOfRangeException("Packet longer than 32768 bytes");

                int IndexserSize = packetLength > 0x7f ? 2 : 1;

                if(_packetContainer == null || _packetContainer.Length != (packetLength+IndexserSize))
                    _packetContainer = new byte[packetLength + IndexserSize];

                if (IndexserSize > 1)
                {
                    _packetContainer[1] = (byte) packetLength;
                    _packetContainer[0] = (byte)((packetLength>>8) | 0x80);
                }
                else
                {
                    _packetContainer[0] = (byte)packetLength;
                }

                int rxIndex = IndexserSize;
                for (int i = 0; i < _elements.Count; i++)
                {
                    _packetContainer[rxIndex] = CommandCollection.Instance.GetCommand(_elements[i].GetType());
                    byte[] packetData = _elements[i].Data;
                    Array.Copy(packetData, 0, _packetContainer, rxIndex+1, packetData.Length);
                    rxIndex += packetData.Length + 1;
                }

                return _packetContainer;
            }
        }

        internal static int GetPacketLength(byte[] packetPart)
        {
            int ret = packetPart[1] + ((int)(packetPart[0] << 8) & 0x7F00);
            return ret;
        }

        public List<PacketElement> DecomposePacket(byte[] payload, int startIndex)
        {
            List<PacketElement> elements = new List<PacketElement>();

            for (int i = startIndex; i < payload.Length; i++)
            {
                PacketElement element = CommandCollection.Instance.CreateElement(payload[i]);
                int length = 0;
                if (element.ExpectedSize == Size.Dynamic)
                    length = (payload.Length) - (i+1);
                else
                    length = element.ExpectedSize;

                byte[] data = new byte[length];
                Array.Copy(payload,(i+1),data,0,length);

                element.Data = data;

                elements.Add(element);
                i += length; 
            }

            return elements; 
        }

    }
}