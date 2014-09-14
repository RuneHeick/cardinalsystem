using CardinalTypes.CardinalGroud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalTypes.Util
{
    interface ICardenalCollider
    {

        //is must have identifyers 
        List<string> IdentifyerNames { get; } 

        Action<CardinalGroup, CardinalGroup> smatch { get;  } 

    }
}
