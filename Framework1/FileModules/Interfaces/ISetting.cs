using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileModules.Interfaces
{
    internal interface ISetting
    {
        string Name { get; }

        string Groupe { get; }

        void SetValue(string value);

    }
}
