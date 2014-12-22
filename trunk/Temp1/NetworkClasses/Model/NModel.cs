using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class NModel
    {
        private Dictionary<string, object> Properties = new Dictionary<string, object>(); 

        public uint ID { get; private set; }

        public NModel(uint ID)
        {
            this.ID = ID; 
        }

        public object this[string name]
        {
            get
            {
                string[] levels = name.Split('.');
                if (Properties.ContainsKey(levels[0]))
                {
                    object obj = Properties[levels[0]];
                    if (levels.Length > 1)
                    {
                        var nModel = obj as NModel;
                        if (nModel != null)
                        {
                            return nModel[name.Substring(levels[0].Length) + 1];
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("The Proporty " + name + " does not exsist");
                        }
                    }
                    else
                    {
                        return obj;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The Proporty " + name + " does not exsist");
                }

            }
            set
            {

            }

        }

    }
}
