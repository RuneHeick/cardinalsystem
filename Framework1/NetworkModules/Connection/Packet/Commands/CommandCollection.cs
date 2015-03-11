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
            GetAll();
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

        private void GetOrCreateCommand<T>(string name) where T: PacketElement, new()
        {
            lock (_commandsDictionary)
            {
                ICommandId command = GetCommandId(name);
                if (null != command)
                    return;

                for (byte id = (byte) _commandsDictionary.Count; id < 255; id++)
                {
                    if (!_commandsDictionary.ContainsKey(id))
                    {
                        ICommandId com = new CommandId<T>(name, id);
                        _commandsDictionary.Add(id, com); 
                        return; 
                    }
                }

                throw new IndexOutOfRangeException("To Manny Commands Created");
            }
        }

        public void GetAll()
        {
            var type = typeof (PacketElement);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
            var list = types.ToList();
            foreach (var item in list)
            {
                try
                {
                    MethodInfo method = typeof (CommandCollection).GetMethod("GetOrCreateCommand");
                    MethodInfo generic = method.MakeGenericMethod(item);

                    PacketElement instance = (PacketElement) Activator.CreateInstance(item);

                    generic.Invoke(this, new object[] {item.Name});
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }


        internal byte GetCommand(Type type)
        {
            throw new NotImplementedException();
        }

        public PacketElement CreateElement(byte b)
        {
            throw new NotImplementedException();
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
