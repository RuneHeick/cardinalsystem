using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BaseInterfaces;
using FileModules.Event;
using FileModules.Interfaces;

namespace FileModules
{
    public class SettingManager : IModule, ISettingManager
    {
        public Type[] SupportedInterface
        {
            get { return typeof (SettingManager).GetInterfaces(); }
        }

        private readonly List<WeakReference> _settings = new List<WeakReference>();
        private readonly string _path;
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

        public SettingManager(string path)
        {
            _path = path;
            if (!File.Exists(_path))
                using (var w = File.CreateText(_path))
                {
                    w.WriteLine("#Configuration File");
                }

            Setupwatcher();
        }

        public Setting<T> GetSetting<T>(string name, string groupe)
        {
            lock (_settings)
            {
                var setting = SearchsettingsList<T>(name, groupe);
                if (setting != null)
                    return setting;

                setting = GetFromFile<T>(name, groupe);
                _settings.Add(new WeakReference(setting));
                return setting;
            }
        }

        public Setting<T> GetOrCreateSetting<T>(string name, string groupe, T baseValue)
        {
            lock (_settings)
            {
                var setting = GetSetting<T>(name, groupe);
                if (setting != null)
                    return setting;
                setting = new Setting<T>(name, groupe, baseValue, SaveSetting);
                _settings.Add(new WeakReference(setting));
                SaveSetting(setting);
                return setting;
            }
        }

        private Setting<T> GetFromFile<T>(string name, string groupe)
        {
            lock (_settings)
            {
                bool foundGrup = false;
                string[] fileLines = File.ReadAllLines(_path);

                foreach (string line in fileLines)
                {
                    if (line.TrimStart().StartsWith("#") || line.TrimStart().Equals(""))
                        continue;

                    string[] info = line.Split('=');

                    if (info.Length == 1)
                    {
                        if (info[0] == groupe)
                            foundGrup = true;
                        else if (foundGrup)
                        {
                            foundGrup = false;
                            break;
                        }
                    }

                    if (info.Length > 1)
                    {
                        if (info[0] == name && foundGrup)
                        {
                            try
                            {
                                var setting = new Setting<T>(name, groupe, (T) Convert.ChangeType(info[1], typeof (T)),
                                    SaveSetting);
                                return setting;
                            }
                            catch (Exception)
                            {
                                return null;
                            }
                        }
                    }
                }
                return null;
            }
        }

        private void SaveSetting(ISetting setting)
        {

            lock (_settings)
            {
                try
                {
                    _watcher.EnableRaisingEvents = false;
                    string[] fileLines = File.ReadAllLines(_path);
                    using (var newfile = File.CreateText(_path))
                    {
                        bool foundGroup = false;
                        bool isDone = false;
                        foreach (string line in fileLines)
                        {
                            if (line.TrimStart().StartsWith("#") || line.TrimStart().Equals(""))
                            {
                                newfile.WriteLine(line);
                                continue;
                            }


                            string[] info = line.Split('=');

                            // din't find a config
                            if (foundGroup && info.Length == 1)
                            {
                                newfile.WriteLine(setting.Name + "=" + setting.ToString());
                                isDone = true;
                                foundGroup = false;
                            }
                            // din't find a config
                            else if (info.Length == 1 && info[0] == setting.Groupe)
                            {
                                foundGroup = true;
                                newfile.WriteLine(line);
                            }
                            else if (info.Length > 1 && foundGroup)
                            {
                                // found list 
                                if (info[0] == setting.Name)
                                {
                                    // setting place.
                                    newfile.WriteLine(setting.Name + "=" + setting.ToString());
                                    isDone = true;
                                }
                                else
                                {
                                    newfile.WriteLine(line);
                                }
                            }
                            else
                            {
                                newfile.WriteLine(line);
                            }
                        }
                        if (!isDone)
                        {
                            if (foundGroup)
                            {
                                newfile.WriteLine(setting.Name + "=" + setting.ToString());
                            }
                            else
                            {
                                newfile.WriteLine(setting.Groupe);
                                newfile.WriteLine(setting.Name + "=" + setting.ToString());
                            }
                        }
                    }
                }
                catch
                {

                }
                finally
                {
                    _watcher.EnableRaisingEvents = true;
                }
            }
        }

        private Setting<T> SearchsettingsList<T>(string name, string groupe)
        {
            for (int i = 0; i < _settings.Count; i++)
            {
                if (_settings[i].Target == null || _settings[i].IsAlive == false)
                {
                    _settings.RemoveAt(i);
                    i--;
                    continue;
                }

                var item = _settings[i].Target as Setting<T>;
                if (item != null && item.Name == name && item.Groupe == groupe)
                {
                    return item;
                }
            }

            return null;
        }

        private void Setupwatcher()
        {
            _watcher.Path = Path.GetDirectoryName(Path.GetFullPath(_path));
            _watcher.Filter = Path.GetFileName(_path);
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += new FileSystemEventHandler(FileChanged);
            _watcher.EnableRaisingEvents = true;
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                lock (_settings)
                {
                    string currentGroupe = null;
                    string[] fileLines = File.ReadAllLines(_path);

                    foreach (string line in fileLines)
                    {
                        if (line.TrimStart().StartsWith("#") || line.TrimStart().Equals(""))
                            continue;

                        string[] info = line.Split('=');

                        if (info.Length == 1)
                        {
                            currentGroupe = info[0];
                        }

                        if (info.Length > 1 && currentGroupe != null)
                        {
                            string name = info[0];
                            var focusSetting = _settings.FirstOrDefault((o) =>
                            {
                                if (o.Target != null)
                                {
                                    var setting = o.Target as ISetting;
                                    if (setting != null && setting.Name == name && setting.Groupe == currentGroupe &&
                                        setting.ToString() != info[1])
                                        return true;
                                }

                                return false;
                            });

                            if (focusSetting != null)
                            {
                                var item = focusSetting.Target as ISetting;
                                if (item != null)
                                    item.SetValue(info[1]);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public void Start()
        {
            
        }

        public void Dispose()
        {
            
        }

    }

    public sealed class Setting<T> : ISetting
    {
        private T _value;

        internal Setting(string name, string groupe, T value, Action<ISetting> saveAction)
        {
            Name = name;
            Groupe = groupe;
            _value = value;
            _saveAction = saveAction;
        }

        private Action<ISetting> _saveAction;

        public string Name { get; private set; }

        public string Groupe { get; private set; }

        public void SetValue(string value)
        {
            try
            {
                Value = (T) Convert.ChangeType(value, typeof (T));
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public T Value
        {
            get { return _value; }
            set
            {
                if (_value.Equals(value))
                    return;
                var old = _value;
                _value = value;
                _saveAction(this);
                OnSettingChanged(old);
            }
        }

        public event SettingHandler<T> SettingChanged;

        public override string ToString()
        {
            return Value.ToString();
        }

        private void OnSettingChanged(T oldValue)
        {
            var handler = SettingChanged;
            if (handler != null)
            {
                SettingEventArgs<T> e = new SettingEventArgs<T>(this, oldValue);
                handler(this, e);
            }
        }
    }



}
