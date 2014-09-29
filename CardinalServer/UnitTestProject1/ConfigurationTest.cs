using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConfigurationManager;
using System.IO;
using System.Threading;

namespace UnitTestProject1
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void BaseFunction()
        {
            
            ConfigurationController rune = ConfigurationController.Instance;
            File.Delete("Configurations.txt"); 
            {
                {
                    Configuration config = rune.GetConfig("Rune", "UnitTest");
                    Configuration config2 = rune.GetConfig("UnitTest2", "Rune");
                    config.SetValue("hej med dig2");
                    config2.SetValue("2");
                }
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForFullGCComplete(); 
                GC.WaitForPendingFinalizers();
                {
                    Configuration config = rune.GetConfig("Rune","UnitTest");
                    Configuration config2 = rune.GetConfig("UnitTest2", "Rune");

                    Assert.AreEqual("hej med dig2", config.ToString());
                    Assert.AreEqual("2", config2.ToString()); 
                }
            }
        }

        [TestMethod]
        public void GetNewList()
        {
            File.Delete("Configurations.txt"); 
            ConfigurationController rune = ConfigurationController.Instance;
            {
                {
                    var config = rune.GetConfigurations("UnitTest2000");
                    Assert.AreEqual(config.Count, 0);
                }
            }
        }

        [TestMethod]
        public void GetOldList()
        {
            File.Delete("Configurations.txt"); 
            {
                ConfigurationController rune = ConfigurationController.Instance;
                Configuration config = rune.GetConfig("Test5", "UnitTest");
                config.SetValue("Test");
             }

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers(); 
                
            {
               ConfigurationController rune = ConfigurationController.Instance;
               var hest = rune.GetConfigurations("UnitTest");
               Assert.AreNotEqual(hest.Count, 0);
            }
        }



    }
}
