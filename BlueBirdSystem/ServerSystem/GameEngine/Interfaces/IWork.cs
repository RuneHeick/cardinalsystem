using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Interfaces
{
    public interface IWork
    {


        uint? Tick(uint tick);
    }
}
