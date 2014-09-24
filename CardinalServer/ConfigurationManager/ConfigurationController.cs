using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; 


namespace ConfigurationManager
{
    public sealed class ConfigurationController
    {
        private static volatile ConfigurationController instance;
        private static object syncRoot = new Object();
        private readonly object FileLock = new object(); 

        const string ConfigurationPath = "Configurations.txt";
        static string[] FileContent = null;

        Dictionary<string, WeakReference> LoadedConfigurations = new Dictionary<string, WeakReference>(); 

        private ConfigurationController() 
        {
            
        }


        private string[] ReadFile()
        {
            FileInfo ConfigFile = new FileInfo(ConfigurationPath);
            if (ConfigFile.Exists)
            {
                try
                {
                    return File.ReadAllLines(ConfigFile.FullName);
                }
                catch
                {
                    // Just fall down to null; 
                }
            }

            return null; 
        }


        public Configuration GetConfig(string Name)
        {
            lock (FileLock)
            {
                if (LoadedConfigurations.ContainsKey(Name))
                {
                    WeakReference item = LoadedConfigurations[Name];
                    if (item.Target != null && item.IsAlive == true)
                    {
                        return item.Target as Configuration;
                    }
                }

                Configuration data = GetConfiguration(Name);
                LoadedConfigurations.Add(Name, new WeakReference(data));
                return data; 
            }
        }

        private Configuration GetConfiguration(string Name)
        {
            lock (FileLock)
            {
                string[] fileLines = ReadFile();
                if (fileLines != null)
                {
                    foreach (string line in fileLines)
                    {
                        if (line.TrimStart().StartsWith("#"))
                            continue;

                        string[] info = line.Split('=');
                        if (info.Length > 1)
                        {
                            if (info[0] == Name)
                            {
                                Configuration ret = new Configuration(SetConfigurationFile);
                                ret.Name = info[0];
                                ret.SetValue(info[1]);
                                return ret;
                            }
                        }
                    }
                }
                {
                    Configuration ret = new Configuration(SetConfigurationFile);
                    ret.Name = Name;
                    ret.SetValue(0);
                    return ret;
                }

            }

        }

        private void SetConfigurationFile(Configuration config)
        {
            lock (FileLock)
            {
                if (LoadedConfigurations.Remove(config.Name))
                    LoadedConfigurations.Remove(config.Name);

                try
                {
                    string[] fileLines = ReadFile();
                    for (int i = 0; i < fileLines.Length; i++)
                    {
                        if (fileLines[i].TrimStart().StartsWith("#"))
                            continue;

                        string[] info = fileLines[i].Split('=');
                        if (info.Length > 1)
                        {
                            if (info[0] == config.Name)
                            {
                                string value = config.ToString();
                                fileLines[i] = config.Name + "=" + value;
                                break;
                            }
                        }
                    }
                    File.WriteAllLines(ConfigurationPath, fileLines);
                }
                catch
                {

                }
            }
        }

        public static ConfigurationController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new ConfigurationController();
                    }
                }

                return instance;
            }
        }
    }
}
