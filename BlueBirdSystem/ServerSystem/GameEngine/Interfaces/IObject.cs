using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Interfaces
{
    public enum ChangeItem
    {
        LOCATION_CHANGED
    }

    public delegate void ObjectChangeHandler(IObject sender, ChangeItem change, object item);

    public interface IObject : IWork
    {
        Location Location { get; }

        uint ObjectId { get; }

        event ObjectChangeHandler ObjectChanged;
    }
}
