using GameEngine;
using GameEngine.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [TestClass]
    public class GameEngineTest
    {

        [TestMethod]
        public void CreateAstroid()
        {
            Astroide astroide = new Astroide();
            Area area = new Area();

            area.Add(astroide);

            for (uint i = 0; i < 5000; i++)
            {
                area.Tick(i).Wait();
            }
        }
    }
}
