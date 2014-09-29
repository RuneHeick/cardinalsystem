using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CardinalTypes.Data;

namespace UnitTestProject1
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DataManagerTest1
    {

        [TestMethod]
        public void DataManagerHeap()
        {
            WeakReference t1;
            WeakReference t2; 

            DataManager rune = new DataManager("test");
            TestItem a = new TestItem();
            rune.Add(a);
            t1 = new WeakReference(a);

            var b = new TestItem(); 
            rune.Add(b);
            t2 = new WeakReference(b);
            b = null; 

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            Assert.IsFalse(t2.IsAlive);
            Assert.IsTrue(t1.IsAlive); 
            
        }
    }

    class TestItem : ManagedData
    {
        public override string Serilizer
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    
}
