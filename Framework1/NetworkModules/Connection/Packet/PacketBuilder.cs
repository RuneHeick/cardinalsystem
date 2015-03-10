using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetworkModules.Connection.Packet
{
    class PacketBuilder
    {
        public const int IndexserSize = 2;
        public const int MaxPacketSize = 0x0000ffff; 

        private readonly List<PacketElement> _elements = new List<PacketElement>();
        private readonly CommandCollection _commandCollection; 

        private readonly object _syncRoot = new object();
        private byte[] _packetContainer = null;

        public PacketBuilder(CommandCollection commandCollection)
        {
            _commandCollection = commandCollection;
        }

        public ReadOnlyCollection<PacketElement> Elements
        {
            get { return _elements.AsReadOnly(); }
        }

        public void Add(PacketElement element)
        {
            lock (_syncRoot)
            {
                if (element.IsFixedSize)
                    _elements.Insert(0, element);
                else
                {
                    if (_elements.Count == 0 || _elements[_elements.Count - 1].IsFixedSize)
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
                var packetLength = IndexserSize + (elementsCount - 1) + dataLength;

                if (packetLength > MaxPacketSize)
                    throw new IndexOutOfRangeException("Packet longer than 32768 bytes");

                if(_packetContainer == null || _packetContainer.Length != packetLength)
                    _packetContainer = new byte[packetLength];

                _packetContainer[1] = (byte)packetLength;
                _packetContainer[0] = (byte)(packetLength >> 8);

                int rxIndex = IndexserSize;
                for (int i = 0; i < _elements.Count; i++)
                {
                    _packetContainer[rxIndex] = _elements[i].Type.Command;
                    byte[] packetData = _elements[i].Data;
                    Array.Copy(packetData, 0, _packetContainer, rxIndex+1, packetData.Length);
                    rxIndex += packetData.Length + 1;
                }

                return _packetContainer;
            }
        }

        internal static int GetPacketLength(byte[] packetPart)
        {
            return packetPart[1] + ((packetPart[0]) << 8);
        }

        public List<PacketElement> DecomposePacket(byte[] payload, int startIndex)
        {
            List<PacketElement> elements = new List<PacketElement>();

            for (int i = startIndex; i < payload.Length; i++)
            {
                ICommandId command = _commandCollection.GetCommandId(payload[i]);
                int length = 0;
                if (command.Length == CommandId<PacketElement>.Dynamic)
                    length = (i + 1) - payload.Length;
                else
                    length = command.Length;

                byte[] data = new byte[length];
                Array.Copy(payload,(i+1),data,0,length);

                PacketElement element = command.CreateElement();
                element.Data = data;
                element.IsFixedSize = (command.Length != CommandId<PacketElement>.Dynamic);
                element.Type = command;

                elements.Add(element);

                i += length; 
            }

            return elements; 
        }

    }
}