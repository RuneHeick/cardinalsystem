using System.Collections.Generic;

namespace Utility
{
    public class BiDictionary<TFirst, TSecond>
    {
        private readonly IDictionary<TFirst, IList<TSecond>> _firstToSecond = new Dictionary<TFirst, IList<TSecond>>();
        private readonly IDictionary<TSecond, IList<TFirst>> _secondToFirst = new Dictionary<TSecond, IList<TFirst>>();

        private static readonly IList<TFirst> EmptyFirstList = new TFirst[0];
        private static readonly IList<TSecond> EmptySecondList = new TSecond[0];

        public void Add(TFirst first, TSecond second)
        {
            IList<TFirst> firsts;
            IList<TSecond> seconds;
            if (!_firstToSecond.TryGetValue(first, out seconds))
            {
                seconds = new List<TSecond>();
                _firstToSecond[first] = seconds;
            }
            if (!_secondToFirst.TryGetValue(second, out firsts))
            {
                firsts = new List<TFirst>();
                _secondToFirst[second] = firsts;
            }
            seconds.Add(second);
            firsts.Add(first);
        }

        public bool ContainsKey(TFirst key)
        {
            return _firstToSecond.ContainsKey(key);
        }

        public bool ContainsKey(TSecond key)
        {
            return _secondToFirst.ContainsKey(key);
        }

        // Note potential ambiguity using indexers (e.g. mapping from int to int)
        // Hence the methods as well...
        public IList<TSecond> this[TFirst first]
        {
            get { return GetByFirst(first); }
        }

        public IList<TFirst> this[TSecond second]
        {
            get { return GetBySecond(second); }
        }

        public IList<TSecond> GetByFirst(TFirst first)
        {
            IList<TSecond> list;
            if (!_firstToSecond.TryGetValue(first, out list))
            {
                return EmptySecondList;
            }
            return new List<TSecond>(list); // Create a copy for sanity
        }

        public IList<TFirst> GetBySecond(TSecond second)
        {
            IList<TFirst> list;
            if (!_secondToFirst.TryGetValue(second, out list))
            {
                return EmptyFirstList;
            }
            return new List<TFirst>(list); // Create a copy for sanity
        }
    }
}
