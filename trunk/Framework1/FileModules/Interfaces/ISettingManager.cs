using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileModules
{
    public interface ISettingManager
    {
        Setting<T> GetSetting<T>(string name, string groupe);
    
        Setting<T> GetOrCreateSetting<T>(string name, string groupe, T baseValue);

    }
}
