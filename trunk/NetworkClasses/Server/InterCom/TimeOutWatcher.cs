using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading; 

namespace Server.InterCom
{
    public class TimeOut
    {

        #region Static

        private static OrderedList<TimeOut> list = new OrderedList<TimeOut>(new sortTimeOutInfo());
        private static Timer timer = new Timer(Timer_Tick, null,Timeout.Infinite, Timeout.Infinite);

        public static TimeOut Create<T>(uint TimeOut, T state, Action<T> callback)
        {
            var item = new TimeOut();
            item.TimeoutReset = TimeOut*TimeSpan.TicksPerMillisecond;
            item.TimeoutEndTime = DateTime.Now.Ticks + item.TimeoutReset;
            if (callback != null)
                item.TimeoutAction = () => callback(state);

            lock (list)
            {
                list.Add(item);
            }
            CalibrateTick();
            return item;
        }

        private static void CalibrateTick()
        {
            lock (list)
            {
                if (list.Count > 0)
                {
                    
                    var item = list[0] as TimeOut;
                    lock (item.Lock)
                    {
                        var Tick_WaitTime = (item.TimeoutEndTime - DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
                        if (Tick_WaitTime < 0)
                            Tick_WaitTime = 0;
                        timer.Change(Tick_WaitTime, Timeout.Infinite);
                    }
                }
                else
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        private static void Timer_Tick(object state)
        {
            var now = DateTime.Now.Ticks;
            lock (list)
            {
                for (int i = 0; i < list.Count; i++ )
                {
                    TimeOut item = list[i];
                    lock (item.Lock)
                    {
                        if (now > item.TimeoutEndTime)
                        {
                            list.Remove(item);
                            i--;
                            if (item.TimeoutAction != null)
                             Task.Factory.StartNew((() => 
                             {
                                 lock (item.Lock)
                                 {
                                     if (item.TimeoutAction != null)
                                        item.TimeoutAction();
                                     item.TimeoutAction = null;
                                 }
                             }));
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                CalibrateTick();
            }
            
        }

        private static void UpdateTouch(TimeOut item)
        {
            lock(list)
            {
                if (list.Remove(item))
                {
                    list.Add(item);
                    CalibrateTick();
                }
            }
        }

        #endregion
        class sortTimeOutInfo : IComparer<TimeOut>
        {

            public int Compare(TimeOut x, TimeOut y)
            {
                if (x.TimeoutEndTime < y.TimeoutEndTime)
                    return -1;
                if (x.TimeoutEndTime > y.TimeoutEndTime)
                    return 1;
                return 0;
            }

        }



        #region Unit

        private long TimeoutEndTime;
        private long TimeoutReset;
        private readonly object Lock = new object();

        private Action TimeoutAction = null;


        private TimeOut()
        {}

        public void Touch()
        {
            lock (Lock)
            {
                TimeoutEndTime = DateTime.Now.Ticks + TimeoutReset;
            }
            UpdateTouch(this);
        }


        public void Calcel()
        {
            lock (list)
            {
                if (list.Remove(this))
                    CalibrateTick();
            }
            lock(this.Lock)
                TimeoutAction = null; 
        }

        #endregion
    }

}
