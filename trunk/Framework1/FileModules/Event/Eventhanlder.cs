using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileModules.Event
{

    public delegate void SettingHandler<T>(object sender, SettingEventArgs<T> e);

    public class SettingEventArgs<T> : EventArgs
    {
        public SettingEventArgs(Setting<T> setting, T oldValue)
        {
            Setting = setting;
            OldValue = oldValue;
        }

        public Setting<T> Setting { get; private set; }

        public T OldValue { get; private set;  }
    }
}
