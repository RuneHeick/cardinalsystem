using MatrixSystem.ObjectCreation;
using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem.Network
{
    class NetworkUpdateHandler
    {
        internal static List<byte[]> GetFullHeader(GameObject gameObject)
        {
            var ret = new List<byte[]>();
            ret.Add(BitConverter.GetBytes(gameObject.ObjectID));
            ret.Add(BitConverter.GetBytes(GameObjectCreator.getObjectHash(gameObject.GetType())));
            return ret;
        }

        internal static List<byte[]> GetSyncHeader(GameObject gameObject)
        {
            var ret = new List<byte[]>();
            ret.Add(BitConverter.GetBytes(gameObject.ObjectID));
            return ret; 
        }
    }
}
