using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server3.Intercom.SharedFile;

namespace Server3.Intercom.SharedFile2
{
    class SafeCollection<TK,TV>
    {
        private readonly Dictionary<TK, TV> _collection; 

        public SafeCollection(IEqualityComparer<TK> equalityComparer = null)
        {
            if (equalityComparer != null)
                _collection = new Dictionary<TK, TV>(equalityComparer);
            else
                _collection = new Dictionary<TK, TV>();
        }


        public bool ContainsFile(TK name)
        {
            lock (_collection)
            {
                return _collection.ContainsKey(name);
            }
        }

        public TV this[TK name]
        {
            get
            {
                lock (_collection)
                {
                    if (_collection.ContainsKey(name))
                        return _collection[name];
                }
                return default(TV); 
            }
            set
            {
                lock (_collection)
                {
                    if (_collection.ContainsKey(name))
                        _collection[name] = value;
                    else
                        _collection.Add(name, value);
                }
            }
        }


        public void Remove(TK name)
        {
            lock (_collection)
            {
                if (_collection.ContainsKey(name))
                    _collection.Remove(name);
            }
        }

    }
}
