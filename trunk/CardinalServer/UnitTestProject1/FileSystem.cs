using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManager;
using System.IO;
using ConfigurationManager;

namespace UnitTestProject1
{
    /// <summary>
    /// Summary description for FileSystem
    /// </summary>
    [TestClass]
    public class FileSystem
    {
        [TestMethod]
        public void FileSystemAccessFail()
        {
            File.Delete("Configurations.txt"); 
            FileController con = FileController.Instance;
            byte[] data = new byte[]{(byte)'A', (byte)'B', (byte)'C'}; 

            try
            {
                con.Save("C:\\test.txt", data);
                Assert.Fail(); 
            }
            catch
            {
                
            }

        }

        [TestMethod]
        public void FileSystemAccessOK()
        {
            File.Delete("Configurations.txt");
            
            FileController con = FileController.Instance;
            Configuration a = ConfigurationManager.ConfigurationController.Instance.GetConfig("Drive5", FileController.Configurations);
            a.SetValue("C:\\");

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForFullGCComplete();
            GC.WaitForPendingFinalizers();

            byte[] data = new byte[] { (byte)'A', (byte)'B', (byte)'C' };

            try
            {
                con.Save("C:\\test.txt", data);
                
            }
            catch
            {
                Assert.Fail();
            }

        }


    }
}
