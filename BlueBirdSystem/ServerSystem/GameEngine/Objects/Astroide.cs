using GameEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Objects
{
    public class Astroide : IObject
    {
        private uint _nextTick = 0;

        public Location Location { get; private set; } = new Location() { ToX = 0.5f, ToY = 0.5f, ToTick = 0 };

        public uint ObjectId { get; } = 10;

        public event ObjectChangeHandler ObjectChanged;

        public uint? Tick(uint tick)
        {
            if(tick >= _nextTick)
            {
                _nextTick = tick+5;
                Location.FromTick = tick;
                Location.FromX = Location.ToX;
                Location.FromY = Location.ToY;

                Random ran = new Random();
                Location.ToX = (float)ran.NextDouble();
                Location.ToY = (float)ran.NextDouble();

                Location.ToTick = _nextTick;

                ObjectChanged?.Invoke(this, ChangeItem.LOCATION_CHANGED, Location);
            }
            return _nextTick;
        }
    }
}
