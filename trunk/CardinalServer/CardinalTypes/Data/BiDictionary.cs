using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Data
{
    class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public void Add(TFirst first, TSecond second)
        {
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public long Count
        {
            get
            {
                return firstToSecond.LongCount(); 
            }
        }

        public bool Contains(TFirst first)
        {
            return firstToSecond.ContainsKey(first); 
        }

        public bool Contains(TSecond second)
        {
            return secondToFirst.ContainsKey(second);
        }

        // Note potential ambiguity using indexers (e.g. mapping from int to int)
        // Hence the methods as well...
        public TSecond this[TFirst first]
        {
            get { return firstToSecond[first]; }
        }

        public TFirst this[TSecond second]
        {
            get { return secondToFirst[second]; }
        }

        public void Remove(TSecond second)
        {
            TFirst temp = secondToFirst[second];
            secondToFirst.Remove(second);
            firstToSecond.Remove(temp);
        }

        public void Remove(TFirst first)
        {
            TSecond temp = firstToSecond[first];
            firstToSecond.Remove(first);
            secondToFirst.Remove(temp);
        }

    }
}
