using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkModules.Connection.Packet.Commands
{
    public class CommandCollection
    {
        private readonly Dictionary<byte, ICommandId> _commandsDictionary = new Dictionary<byte, ICommandId>();
        private static volatile CommandCollection _instance;
        private static readonly object syncRoot = new Object();
        public static CommandCollection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new CommandCollection();
                    }
                }

                return _instance;
            }
        }

        private CommandCollection()
        {

        }

        private ICommandId GetCommandId(string name)
        {
            lock (_commandsDictionary)
            {
                var values = _commandsDictionary.Values;
                var com = values.FirstOrDefault((o) => o.Name == name);
                if (com != null)
                    return com;
            }
            return null;
        }

        public void GetOrCreateCommand<T>() where T: PacketElement, new()
        {
            lock (_commandsDictionary)
            {
                ICommandId command = GetCommandId(typeof(T).Name);
                if (null != command)
                    return;

                for (byte id = (byte) _commandsDictionary.Count; id < 254; id++)
                {
                    if (!_commandsDictionary.ContainsKey(id))
                    {
                        ICommandId com = new CommandId<T>(typeof(T).Name, id);
                        _commandsDictionary.Add(id, com); 
                        return; 
                    }
                }

                throw new IndexOutOfRangeException("To Manny Commands Created");
            }
        }

        public void CreateProtocolDefinition()
        {
            var type = typeof (PacketElement);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            var list = types.ToList();

            MethodInfo method = typeof(CommandCollection).GetMethod("GetOrCreateCommand");

            foreach (var item in list)
            {
                try
                {
                    
                    MethodInfo generic = method.MakeGenericMethod(item);

                    PacketElement instance = (PacketElement) Activator.CreateInstance(item);

                    generic.Invoke(this, null);
                }
                catch (Exception e)
                {
                    throw new NotSupportedException("The Element "+item.Name+" Can not be created");
                }
            }
        }

        public void ResetCommands()
        {
            lock(_commandsDictionary)
                _commandsDictionary.Clear();
        }

        internal byte GetCommand(Type type)
        {
            var id = GetCommandId(type.Name);
            if (id == null)
                return 255;
            else
                return id.Command;
        }

        public PacketElement CreateElement(byte b)
        {
            lock (_commandsDictionary)
            {
                if (_commandsDictionary.ContainsKey(b))
                {
                    return _commandsDictionary[b].CreateElement();
                }
            }
            return null;
        }

        private class CommandId<T> : ICommandId where T : PacketElement, new()
        {
            public CommandId(string name, byte command)
            {
                Name = name;
                Command = command;
            }

            public byte Command { get; private set; }

            public string Name { get; private set; }

            public PacketElement CreateElement()
            {
                return new T();
            }

            public Type ElementType
            {
                get { return typeof (T); }
            }
        }

    }
 

}
