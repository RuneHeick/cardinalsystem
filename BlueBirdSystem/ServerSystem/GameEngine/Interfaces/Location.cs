using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Interfaces
{
    public class Location
    {
        public float ToX;
        public float ToY;
        public float FromX;
        public float FromY;

        public uint FromTick;
        public uint ToTick;
    }
}
