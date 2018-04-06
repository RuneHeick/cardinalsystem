using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    class SpeedThreadPool
    {
        Queue<Action> _queue;
        Thread[] worker = new Thread[4];
        readonly object done = new object();

        public SpeedThreadPool(int capacity)
        {
            _queue = new Queue<Action>(capacity);
            for (int i = 0; i < worker.Length; i++)
            {
                worker[i] = new Thread(new ThreadStart(doWork));
                worker[i].Priority = ThreadPriority.Highest;
                worker[i].Start();
            }
        }

        private void doWork()
        {
            Action runAction = null;
            lock (_queue)
            {
                while (_queue.Count == 0)
                {
                    lock(done)
                      Monitor.Pulse(done);
                    Monitor.Wait(_queue);
                }
                runAction = _queue.Dequeue();
            }

            runAction();
        }


        public void Enqueue(Action action)
        {
            lock (_queue)
            {
                if (_queue.Count == 0)
                    Monitor.Pulse(_queue);
                _queue.Enqueue(action);
            }
        }

        public void Wait()
        {
            lock (done)
            {
                Monitor.Wait(done);
            }
        }


    }
}
