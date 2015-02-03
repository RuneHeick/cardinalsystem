using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server3
{
    public static class EventBus
    {

        private static IColletionItem[] _colletion = new IColletionItem[10];

        public static void Publich<T>(T item, bool threaded = true)
        {
            if (threaded)
                Task.Factory.StartNew(() => PublichWorker(item));
            else
                PublichWorker(item);
        }

        private static void PublichWorker<T>(T item)
        {
            try
            {
                Monitor.Enter(_colletion);
                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i].Type == typeof(T) || typeof(T).IsAssignableFrom(_colletion[i].Type))
                    {
                        var cItem = (CollectionItem<T>) _colletion[i];
                        Monitor.Exit(_colletion);
                        if (cItem.Condition == null || cItem.Condition(item))
                            cItem.Handler(item);
                        Monitor.Enter(_colletion);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_colletion); 
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
            lock (_colletion)
            {
                for (int i = 0; i < _colletion.Length; i++)
                {
                    if (_colletion[i] != null && _colletion[i].Type == typeof(T))
                    {
                        var cItem = (CollectionItem<T>)_colletion[i];
                        if (cItem.Condition == condition && cItem.Handler == handlerAction)
                            _colletion[i] = null; 
                    }
                }
            }
        }

        private static void Add(IColletionItem item)
        {
            lock (_colletion)
            {
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

    internal interface IColletionItem
    {
        Type Type { get; }
    }
}
