using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkModules.Connection.Packet
{
    public class CommandCollection
    {
        private readonly Dictionary<byte, ICommandId> _commandsDictionary = new Dictionary<byte, ICommandId>();
        private static readonly ICommandId UndefinedId = new CommandId<PacketElement>(CommandId <PacketElement>.Dynamic, "Undefined Command", 0);

        public CommandCollection()
        {
            GetAll();
        }

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

        public ICommandId CreateCommand(string description, int length)
        {
            return GetOrCreateCommand<PacketElement>(description, length);
        }

        public ICommandId GetOrCreateCommand<T>(string description, int length) where T: IPacketElement, new()
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
                        _commandsDictionary.Add(id, com); 
                        return com; 
                    }
                }

                throw new IndexOutOfRangeException("To Manny Commands Created");
            }
        }


        public void GetAll()
        {
            var type = typeof (IPacketElement);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);
            var list = types.ToList();
            foreach (var item in list)
            {
                try
                {
                    MethodInfo method = typeof (CommandCollection).GetMethod("GetOrCreateCommand");
                    MethodInfo generic = method.MakeGenericMethod(item);

                    IPacketElement instance = (IPacketElement) Activator.CreateInstance(item);

                    generic.Invoke(this, new object[] { item.Name, (instance.IsFixedSize ? instance.Length : CommandId<PacketElement>.Dynamic) });
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

    }

    public class CommandId<T>: ICommandId where T : IPacketElement, new()
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

        public IPacketElement CreateElement()
        {
            return new T();
        }

        public static int Dynamic
        {
            get { return 0; }
        }

    }

}
