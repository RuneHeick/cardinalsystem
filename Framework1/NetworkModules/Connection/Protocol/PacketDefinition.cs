using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkModules.Connection
{
    public class PacketDefinition
    {
        public List<Type> ElementTypes { get; set; }

        public PacketDefinition()
        {
            ElementTypes = new List<Type>();
        }

        public PacketDefinition(List<Type> elementTypes)
        {
            ElementTypes = elementTypes;
        }

    }
}
