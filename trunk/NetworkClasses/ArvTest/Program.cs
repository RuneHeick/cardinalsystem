using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ArvTest
{
    class Program
    {
        static void Main(string[] args)
        {
            one i = new one();
            IntercomModule k = new IntercomModule();
            two x = new two();
            while (true)
            {
                
            }

        }
    }

    class IntercomModule
    {
        private static readonly List<IntercomModule> _modues = new List<IntercomModule>();

        public IntercomModule()
        {
            _modues.Add(this);
            OnModuleAdded(this);
        }

        protected static T GetModule<T>() where T : IntercomModule
        {
            var item = _modues.FirstOrDefault((o) => o.GetType() == typeof(T));
            return item as T;
        }

        protected static event Action<IntercomModule> ModuleAdded;

        private static void OnModuleAdded(IntercomModule obj)
        {
            var handler = ModuleAdded;
            if (handler != null) handler(obj);
        }
    }


    class one : IntercomModule
    {
        public one()
        {
            one.ModuleAdded += moduleAdded;
        }

        private void moduleAdded(IntercomModule obj)
        {
            Console.WriteLine(obj.GetType());
        }


    }

    class two : IntercomModule
    {
        public two()
        {
            
        }
        
    }


}
