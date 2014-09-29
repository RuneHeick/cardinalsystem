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

        public Configuration GetConfig(string Name, string group)
        {
            lock (FileLock)
            {
                if (LoadedConfigurations.ContainsKey(Name + group))
                {
                    WeakReference item = LoadedConfigurations[Name + group];
                    if (item.Target != null && item.IsAlive == true)
                    {
                        return item.Target as Configuration;
                    }
                }

                Configuration data = GetConfiguration(Name, group);
                data.Group = group;
                LoadedConfigurations.Add(Name + group, new WeakReference(data));
                return data; 
            }
        }

        private Configuration GetConfiguration(string Name, string group)
        {
            lock (FileLock)
            {
                bool foundGrup = false; 
                string[] fileLines = ReadFile();
                if (fileLines != null)
                {
                    foreach (string line in fileLines)
                    {
                        if (line.TrimStart().StartsWith("#"))
                            continue;

                        string[] info = line.Split('=');

                        if(info.Length == 1)
                        {
                            if(info[0] == group)
                                foundGrup = true; 
                            else
                                foundGrup = false; 
                        }

                        if (info.Length > 1)
                        {
                            if (info[0] == Name && foundGrup == true)
                            {
                                Configuration ret = new Configuration(SetConfigurationFile);
                                ret.Name = info[0];
                                ret.SetValue(info[1]);
                                ret.Group = group; 
                                return ret;
                            }
                        }
                    }
                }
                {
                    Configuration ret = new Configuration(SetConfigurationFile);
                    ret.Name = Name;
                    ret.SetValue(0);
                    ret.Group = group; 
                    return ret;
                }

            }

        }

        private void SetConfigurationFile(Configuration config)
        {
            lock (FileLock)
            {
                if (LoadedConfigurations.Remove(config.Name+config.Group))
                    LoadedConfigurations.Remove(config.Name + config.Group);

                writeConfigurationFile(config);
               
            }
        }

        private void writeConfigurationFile(Configuration config)
        {
            try
            {
                string[] fileLines = ReadFile();

                if (fileLines == null)
                    fileLines = new string[0];


                bool foundGroup = false;

                for (int i = 0; i < fileLines.Length; i++)
                {
                    if (fileLines[i].TrimStart().StartsWith("#"))
                        continue;

                    string[] info = fileLines[i].Split('=');

                    // din't find a config
                    if (foundGroup == true && info.Length == 1)
                    {
                        List<string> items = fileLines.ToList();
                        items.Insert(i, config.Name + "=" + config.ToString());
                        fileLines = items.ToArray();
                        File.WriteAllLines(ConfigurationPath, fileLines);
                        return;
                    }
                    // din't find a config
                    else if (info.Length == 1 && info[0] == config.Group)
                    {
                        foundGroup = true;
                    }
                    else if (info.Length > 1 && foundGroup == true)
                    {
                        if (info[0] == config.Name)
                        {
                            string value = config.ToString();
                            fileLines[i] = config.Name + "=" + value;
                            File.WriteAllLines(ConfigurationPath, fileLines);
                            return;
                        }
                    }
                }
                {
                    List<string> items = fileLines.ToList(); 
                    items.Add(config.Group);
                    items.Add(config.Name + "=" + config.ToString());
                    fileLines = items.ToArray();
                    File.WriteAllLines(ConfigurationPath, fileLines);
                    return;
                }
            }
            catch
            {

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
