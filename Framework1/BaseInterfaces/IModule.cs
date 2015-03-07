using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseInterfaces
{
    public interface IModule: IDisposable
    {
        Type[] SupportedInterface { get; }

        void Start();

    }
}
