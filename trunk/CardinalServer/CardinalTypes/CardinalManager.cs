using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardinalTypes.Util;
using CardinalTypes.CardinalGroud;

namespace CardinalTypes
{
    public class CardinalManager
    {
        List<ICardenalCollider> Colliders { get; set; }
        private readonly object coliderLock_ = new object(); 

        List<CardinalGroup> Items { get; set; }

        public void smatch(CardinalGroup one, CardinalGroup two)
        {
            lock (coliderLock_)
            {
                foreach(ICardenalCollider collider in Colliders)
                {

                    if(HasIdentifyer(one, collider.IdentifyerNames) && HasIdentifyer(two, collider.IdentifyerNames))
                    {
                        collider.smatch(one, two); 
                    }
                }
            }
        }

        private bool HasIdentifyer(CardinalGroup one, List<string> list)
        {
            try
            {
                foreach(ICardinalType type in one.Types)
                {
                    if(!list.Contains(type.IdentifyerName))
                    {
                        return false; 
                    }

                }

                return true; 
            }
            catch
            {
                return false; 
            }
        }

    }
}
