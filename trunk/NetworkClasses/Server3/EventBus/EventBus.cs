using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server3
{
    internal static class EventBus
    {

        private static IColletionItem[] _colletion = new IColletionItem[10];
        private static Dictionary<Type,List<Action<Change, Type>>>  _subscribeNotifyers = new Dictionary<Type, List<Action<Change, Type>>>();

        private static int _counter = 0;
        private static readonly object CounterLock = new object();

        public static void Publich<T>(T item, bool threaded = true)
        {
            if (threaded)
                Task.Factory.StartNew(() => PublichWorker(item));
            else
                PublichWorker(item);
        }

        private static void PublichWorker<T>(T item)
        {
            lock (CounterLock)
            {
                _counter++;
            }

            try
            {
                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i] != null && (_colletion[i].Type == typeof(T) || typeof(T).IsAssignableFrom(_colletion[i].Type)))
                    {
                        var cItem = (CollectionItem<T>) _colletion[i];
                        if (cItem.Condition == null || cItem.Condition(item))
                            cItem.Handler(item);
                    }
                }
            }
            catch
            {
                // ignored
            }

            lock (CounterLock)
            {
                _counter--;
                Monitor.PulseAll(CounterLock);
            }
        }

        public static void Subscribe<T>(Action<T> handlerAction, Func<T, bool> condition = null)
        {

            CollectionItem<T> item = new CollectionItem<T>()
            {
                Handler = handlerAction,
                Condition = condition
            };
            Add(item);
        }

        public static void UnSubscribe<T>(Action<T> handlerAction, Func<T, bool> condition = null)
        {
            lock (CounterLock)
            {

                while (_counter > 0)
                {
                    Monitor.Wait(CounterLock);
                }

                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i] != null && _colletion[i].Type == typeof(T))
                    {
                        var cItem = (CollectionItem<T>)_colletion[i];
                        if (cItem.Condition == condition && cItem.Handler == handlerAction)
                            _colletion[i] = null; 
                    }
                }
                if(_colletion.FirstOrDefault((o)=> o.Type == typeof(T)) == null)
                    InvokeSubscribeNotifyer(Change.Removed, typeof(T));

            }
        }

        public static void AddSubscribeNotifyer(Type type, Action<Change, Type> callback)
        {
            lock (_subscribeNotifyers)
            {
                if (!_subscribeNotifyers.ContainsKey(type))
                    _subscribeNotifyers.Add(type, new List<Action<Change, Type>>());

                var list = _subscribeNotifyers[type];
                if (!list.Contains(callback))
                    list.Add(callback);


                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i] != null && (_colletion[i].Type == type || type.IsAssignableFrom(_colletion[i].Type)))
                    {
                        InvokeSubscribeNotifyer(Change.Added, type);
                    }
                }

            }
        }

        public static void RemoveSubscribeNotifyer(Type type, Action<Change, Type> callback)
        {
            lock (_subscribeNotifyers)
            {
                if (_subscribeNotifyers.ContainsKey(type))
                {
                    var list = _subscribeNotifyers[type];
                    list.Remove(callback);
                }
            }
        }

        private static void InvokeSubscribeNotifyer(Change change,Type type)
        {
            lock (_subscribeNotifyers)
            {
                if (_subscribeNotifyers.ContainsKey(type))
                {
                    var list = _subscribeNotifyers[type];
                    foreach (var action in list)
                    {
                        var action1 = action;
                        Task.Factory.StartNew(() => action1(change,type));
                    }
                }
            }
        }


        private static void Add(IColletionItem item)
        {
            lock (CounterLock)
            {
                while (_counter > 0)
                {
                    Monitor.Wait(CounterLock);
                }

                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i] == null)
                    {
                        _colletion[i] = item; 
                        return;
                    }
                }

                IColletionItem[] newColletion = new IColletionItem[_colletion.Length+5];
                Array.Copy(_colletion, newColletion, _colletion.Length);
                newColletion[_colletion.Length] = item;
                _colletion = newColletion; 
            }
            InvokeSubscribeNotifyer(Change.Added, item.Type);
        }
       
        private class CollectionItem<T>: IColletionItem
        {
            public Action<T> Handler { get; set; }

            public Func<T, bool> Condition { get; set; }

            public Type Type
            {
                get { return typeof (T); }
            }
        }

    }

    public enum Change
    {
        Added,
        Removed
    }

    internal interface IColletionItem
    {
        Type Type { get; }
    }
}
