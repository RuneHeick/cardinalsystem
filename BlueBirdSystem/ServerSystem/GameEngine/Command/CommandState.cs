using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Command
{
    public enum CommandState
    {
        ACTIVE, 
        PAUSED,
        COMPLETE,
    }

    public class CommandStatus
    {
        public CommandState State { get; set; }
        public int TickDelay { get; set; }
    }
}
