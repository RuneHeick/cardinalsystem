using GameEngine;
using GameEngine.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Tests
{
    [TestClass]
    public class MatrixTests
    {

        [TestMethod]
        public void TestGameLoopItem()
        {
            MatrixSystem.GridController con = new MatrixSystem.GridController();
            

            Stopwatch watch = new Stopwatch();
            watch.Start();

            con.UpdateMatrix();

            watch.Stop();

            Console.WriteLine("Elapse: " + watch.ElapsedMilliseconds + " ms");

        }
    }
}
