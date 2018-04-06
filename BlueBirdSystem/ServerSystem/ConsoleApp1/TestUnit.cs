using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    class TestUnit
    {
        static Random ran = new Random();
        public int[] data = new int[5];


        public void Action(TestUnit unit2)
        {
           // Random ran = new Random();
            unit2.data[0] += 80 * ran.Next();
        }

    }
}
