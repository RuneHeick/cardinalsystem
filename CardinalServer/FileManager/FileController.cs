using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security; 

namespace FileManager
{   
    public sealed class FileController
    {
        private static volatile FileController instance;
        private static object syncRoot = new Object();

        private Dictionary<string, object> DiskLocks = new Dictionary<string, object>(); 


        private FileController() 
        {
            CreateDriveLock("F:\\Rune\\Test"); 


        }

        private void CreateDriveLock(string path)
        {
            if (DiskLocks != null)
            {
                DiskLocks.Clear();
                DriveInfo drive = null; 
                try
                {
                    drive = new DriveInfo(Path.GetPathRoot((new FileInfo(path).FullName)));
                    long space = drive.AvailableFreeSpace; // throw a exception if not there. 
                    Console.WriteLine(drive.Name);
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
