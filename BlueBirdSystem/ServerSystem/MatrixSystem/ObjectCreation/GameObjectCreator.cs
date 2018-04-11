using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Security.Cryptography;


namespace MatrixSystem.ObjectCreation
{
    public static class GameObjectCreator
    {
        static Dictionary<UInt16, IObjectCreator> _ObjectCreators = new Dictionary<ushort, IObjectCreator>();
        static Dictionary<Type, UInt16> _ObjectHashes = new Dictionary<Type, UInt16>();

        class ObjectCreator<T> : IObjectCreator where T : GameObject, new()
        {
            public GameObject Create()
            {
                return new T();
            }
        }

        static GameObjectCreator()
        {
            var creatorType = typeof(ObjectCreator<>);
            IList<Type> types = GetInstances<GameObject>();
            foreach (var t in types.OrderBy((p) => p.FullName))
            {
                Type[] typeArgs = { t };
                var makeme = creatorType.MakeGenericType(typeArgs);
                IObjectCreator creator = (IObjectCreator)Activator.CreateInstance(makeme);
                UInt16 hash = GetUniqueHash(t);
                _ObjectCreators.Add(hash, creator);
                _ObjectHashes.Add(t, hash);
            }
        }

        private static IList<Type> GetInstances<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where IsOfType<T>(t) && t.GetConstructor(Type.EmptyTypes) != null
                    select t).ToList();
        }

        private static bool IsOfType<T>(Type t)
        {
            if (t == null)
                return false;
            else if (t == (typeof(T)))
                return true;
            else
                return IsOfType<T>(t.BaseType);
        }

        private static UInt16 GetUniqueHash(Type t)
        {
            HashAlgorithm algorithm = MD5.Create();  //or use SHA256.Create();
            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(t.FullName));
            UInt16 hash = (UInt16)((data[0] << 8) + data[1]);
            while (_ObjectCreators.Keys.Contains(hash))
                hash += 1;
            return hash;
        }

        public static UInt16 getObjectHash(Type t)
        {
            return _ObjectHashes[t];
        }

        public static GameObject CreateNewObject(UInt16 hash)
        {
            return _ObjectCreators[hash].Create();
        }

    }
}
