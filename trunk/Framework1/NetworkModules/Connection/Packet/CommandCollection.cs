using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkModules.Connection.Packet
{
    public class CommandCollection
    {
        private readonly Dictionary<byte, ICommandId> _commandsDictionary = new Dictionary<byte, ICommandId>();
        private static readonly ICommandId UndefinedId = new CommandId<PacketElement>(CommandId <PacketElement>.Dynamic, "Undefined Command", 0);

        public ICommandId GetCommandId(byte id)
        {
            lock (_commandsDictionary)
            {
                if (_commandsDictionary.ContainsKey(id))
                    return _commandsDictionary[id]; 
            }
            return UndefinedId; 
        }

        public ICommandId GetCommandId(string description)
        {
            lock (_commandsDictionary)
            {
                var values = _commandsDictionary.Values;
                var com = values.FirstOrDefault((o) => o.Description == description);
                if (com != null)
                    return com;
            }
            return UndefinedId;
        }

        public ICommandId GetOrCreateCommand(string description, int length)
        {
            return GetOrCreateCommand<PacketElement>(description, length);
        }

        public ICommandId GetOrCreateCommand<T>(string description, int length) where T: PacketElement, new()
        {
            lock (_commandsDictionary)
            {
                ICommandId command = GetCommandId(description);
                if (UndefinedId != command)
                    return command;

                for (byte id = (byte) _commandsDictionary.Count; id < 255; id++)
                {
                    if (!_commandsDictionary.ContainsKey(id))
                    {
                        ICommandId com = new CommandId<T>(length, description, id);
                        return com; 
                    }
                }

                throw new IndexOutOfRangeException("To Manny Commands Created");
            }
        }

    }

    public class CommandId<T>: ICommandId where T : PacketElement, new()
    {
        public CommandId(int length, string description, byte command)
        {
            Length = length;
            Description = description;
            Command = command;
        }

        public byte Command { get; private set; }

        public string Description { get; private set; }

        public int Length { get; private set;  }

        public PacketElement CreateElement()
        {
            return new T();
        }

        public static int Dynamic
        {
            get { return 0; }
        }

    }

}
