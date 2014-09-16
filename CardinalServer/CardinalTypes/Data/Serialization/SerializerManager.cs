using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardinalTypes.Data.Serialization;

namespace CardinalTypes.Data
{
    public sealed class SerializerManager
    {
        private static volatile SerializerManager instance;
        private static object syncRoot = new Object();

        Dictionary<string, ISerializer> Serializers = new Dictionary<string, ISerializer>(); 

        private SerializerManager() { }

        public void Add(ISerializer item)
        {
            if(!Serializers.ContainsKey(item.Name))
                Serializers.Add(item.Name,item); 
        }
        
        public void Remove(ISerializer item)
        {
            if (Serializers.ContainsKey(item.Name))
                Serializers.Remove(item.Name); 
        }

        public bool HasSerilizer(string name)
        {
            return Serializers.ContainsKey(name);
        }

        public byte[] Serilize(object item, string SerilizerName)
        {
            try
            {
                if (Serializers.ContainsKey(SerilizerName))
                {
                    return Serializers[SerilizerName].Serilize(item);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public object Deserilize(byte[] item, string SerilizerName)
        {
            try
            {
                if (Serializers.ContainsKey(SerilizerName))
                {
                    return Serializers[SerilizerName].DeSerilize(item);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static SerializerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SerializerManager();
                    }
                }

                return instance;
            }
        }
    }
}

