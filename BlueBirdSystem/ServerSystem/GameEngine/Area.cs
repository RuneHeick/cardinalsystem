using GameEngine.Interfaces;
using Sector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameEngine
{
    public class Area : ISector
    {
        private readonly object _nextTickLock = new object();
        private uint? _nextTick = null;

        private readonly object _workLiskLock = new object();
        private List<IObject> _workList = new List<IObject>();

        public event SectorStatusChanged OnSectorStatusChanged;
        public event SendMsgHandler OnSendMessage;

        public ulong Address => throw new NotImplementedException();

        /// <summary>
        /// Ticks the area's work. 
        /// </summary>
        /// <param name="tick">The Tick time</param>
        /// <returns>Next time it has to be ticked</returns>
        public async Task<uint?> Tick(uint tick)
        {
            List<Task> tasks = new List<Task>();
            lock (_workLiskLock)
            {
                foreach (IObject work in _workList)
                {
                    tasks.Add(Task.Run(() =>
                    {
                        uint? value = work.Tick(tick);
                        lock (_nextTickLock)
                        {
                            if ((value != null) && (_nextTick == null || value < _nextTick))
                            {
                                _nextTick = value;
                            }
                        }
                    }));
                }
            }
            await Task.WhenAll(tasks);
            return _nextTick;
        }

        public void Add(IObject worker)
        {
            lock(_workLiskLock)
            {
                _workList.Add(worker);
                worker.ObjectChanged += Worker_ObjectChanged;
            }
        }

        public void Remove(IObject worker)
        {
            lock (_workLiskLock)
            {
                _workList.Remove(worker);
                worker.ObjectChanged -= Worker_ObjectChanged;
            }
        }



        private void Worker_ObjectChanged(IObject sender, ChangeItem change, object item)
        {
            
        }

        public byte[] Save()
        {
            throw new NotImplementedException();
        }

        public void Load(byte[] data)
        {
            throw new NotImplementedException();
        }


        public void MessageRecieved(IMsg msg)
        {
            throw new NotImplementedException();
        }

        public void ClientOffline(string clientAddress)
        {
            throw new NotImplementedException();
        }


        public void Tick()
        {
            throw new NotImplementedException();
        }


    }
}
