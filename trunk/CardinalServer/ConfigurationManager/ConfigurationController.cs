using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConfigurationManager
{
    public sealed class ConfigurationController
    {
        private static volatile ConfigurationController instance;
        private static object syncRoot = new Object();
        private readonly object FileLock = new object(); 

        const string ConfigurationPath = "Configurations.txt";
        static string[] FileContent = null;

        private System.Collections.Generic.Directory<string, WeakReference> LoadedConfigurations = new System.Collections.Generic.Directory<string, WeakReference>(); 

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
