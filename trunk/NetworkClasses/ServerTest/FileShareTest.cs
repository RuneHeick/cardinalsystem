using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server3.Intercom.SharedFile;
using Server3.Intercom.SharedFile.Files;

namespace ServerTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class FileShareTest
    {
        public FileShareTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void SystemFile()
        {
            SystemFileIndexFile testFile = new SystemFileIndexFile();

            testFile.AddFileInfo("Test1", 1234);

            SystemFileIndexFile testFile2 = new SystemFileIndexFile();
            testFile2.Data = testFile.Data;

            Assert.IsTrue(testFile2.GetHash("Test1") == 1234); 
        }

        [TestMethod]
        public void HashTest()
        {
            SharedFileManager manager = new SharedFileManager("TestDir", new IPEndPoint(IPAddress.Loopback, 5050));

            using (SystemFileIndexFile file = manager.GetFile<SystemFileIndexFile>("BaseFile", IPAddress.Loopback, true))
            {
                file.AddFileInfo("Test", 2000);
            }

            UInt32 hash1;
            using (SystemFileIndexFile file = manager.GetFile<SystemFileIndexFile>("BaseFile", IPAddress.Loopback, true))
            {
                hash1 = file.Hash;
                if (file.GetHash("Test")!=2000)
                    Assert.Fail("content wrong");
            }

            UInt32 hash2 = manager.GetHash("BaseFile", IPAddress.Loopback);

            if(hash1 != hash2)
                Assert.Fail("Hash Wrong");
        }

        static bool ArraysEqual(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }
            return true;
        }


    }
}
