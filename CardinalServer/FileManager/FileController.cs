using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using ConfigurationManager; 

namespace FileManager
{   
    public sealed class FileController
    {
        public const string Configurations = "File Controller Connfigurations";
        private static volatile FileController instance;
        private static object syncRoot = new Object();

        private Dictionary<string, object> DiskLocks = new Dictionary<string, object>(); 
        private List<string> AllowedRoots = new List<string>(); 

        private FileController() 
        {
            ConfigurationController con = ConfigurationController.Instance; 
            var configs = con.GetConfigurations(Configurations);
            if(configs.Count == 0)
            {
                var config = con.GetConfig("Drive1",Configurations); 
                string cdir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"\\Data"; 
                config.SetValue(cdir);
                AllowedRoots.Add(cdir); 
            }
            else
            {
                foreach(Configuration c in configs)
                {
                    if(c.Name.Contains("Drive"))
                    {
                        AllowedRoots.Add(c.ToString());
                    }
                }
            }
        }
        
        public void Save(string path, byte[] file)
        {
            var drive = new DriveInfo(Path.GetPathRoot((new FileInfo(path).FullName)));
            if (!DiskLocks.ContainsKey(drive.Name))
                CreateDriveLock(path);

            if (IsAllowedPath(path))
            {
                lock (DiskLocks[drive.Name])
                {
                    try
                    {
                        File.WriteAllBytes(path, file); 
                    }
                    catch(Exception e)
                    {
                        throw new IOException("File Error " + e.Message); 
                    }
                }
            }
            else
            {
                throw new AccessViolationException("Not Accees to dir, try to config"); 
            }
        }

        public byte[] Load(string path)
        {
            var drive = new DriveInfo(Path.GetPathRoot((new FileInfo(path).FullName)));
            if (!DiskLocks.ContainsKey(drive.Name))
                CreateDriveLock(path);

            if (IsAllowedPath(path))
            {
                lock (DiskLocks[drive.Name])
                {
                    try
                    {
                        return File.ReadAllBytes(path);
                    }
                    catch (Exception e)
                    {
                        throw new IOException("File Error " + e.Message);
                    }
                }
            }
            else
            {
                throw new AccessViolationException("Not Accees to dir, try to config");
            }
        }

        private bool IsAllowedPath(string path)
        {
            return AllowedRoots.Any((o)=>Path.GetFullPath(path).StartsWith(Path.GetFullPath(o)) );
        }

        private void CreateDriveLock(string path)
        {
            if (DiskLocks != null)
            {
                DriveInfo drive = null; 
                try
                {
                    drive = new DriveInfo(Path.GetPathRoot((new FileInfo(path).FullName)));
                    long space = drive.AvailableFreeSpace; // throw a exception if not there. 

                    if(!DiskLocks.ContainsKey(drive.Name))
                    {
                        DiskLocks.Add(drive.Name, new object()); 
                    }
                }
                catch(SecurityException f)
                {

                }
                catch (UnauthorizedAccessException e)
                {

                }
                catch (ArgumentException e)
                {

                }
                catch (DriveNotFoundException e)
                {

                }
                catch(PathTooLongException e)
                {

                }
                catch(NotSupportedException e)
                {

                }
            }
        }
        
        public static FileController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new FileController();
                    }
                }

                return instance;
            }
        }
    }
}
