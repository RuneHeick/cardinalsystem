using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public byte[] GetCollection()
        {
            lock (_commandsDictionary)
            {
                List<byte> data = new List<byte>();
                var values = _commandsDictionary.Values;

                foreach (var value in values)
                {
                    data.Add(value.Command);
                    data.AddRange(UTF8Encoding.UTF8.GetBytes(value.Name));
                    data.Add(0);
                }
                return data.ToArray();
            }
        }

        public void CheckCollection(byte[] data)
        {
           
            lock (_commandsDictionary)
            {
                var known_values = _commandsDictionary.Values.ToDictionary((o)=>o.Name);

                var items = 0; 
                for (int i = 0; i < data.Length;)
                {
                    byte command = data[i];
                    int endindex = Array.IndexOf(data,(byte) 0, i + 1);
                    string name = UTF8Encoding.UTF8.GetString(data, i + 1, endindex-(i + 1));
                    i = endindex + 1;
                    items++;

                    if (known_values.ContainsKey(name))
                    {
                        known_values[name].Command = command; 
                    }
                    else
                    {
                        throw new NotSupportedException("A Command "+name+" is not supported");
                    }
                }

                if(known_values.Count != items)
                    throw new NotSupportedException("Not the same amount of commands in collection");
            }
        }


        private class CommandId<T> : ICommandId where T : PacketElement, new()
        {
            public CommandId(string name, byte command)
            {
                Name = name;
                Command = command;
            }

            public byte Command { get; set; }

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
