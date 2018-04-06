using System;
using System.Collections.Generic;
using System.Text;

namespace MatrixSystem
{
    class GameObjectCounter
    {
        private static uint counter = 0; 

        public static uint GetUniqueID
        {
            get
            {
                counter++;
                return counter;
            }
        }
    }
}
