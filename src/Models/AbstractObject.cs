using Rampastring.Tools;
using System;

namespace TSMapEditor.Models
{
    public abstract class AbstractObject : INIDefineable
    {
        public abstract RTTIType WhatAmI();

        public bool IsTechno()
        {
            RTTIType rtti = WhatAmI();

            return rtti == RTTIType.Aircraft || 
                   rtti == RTTIType.Infantry || 
                   rtti == RTTIType.Unit || 
                   rtti == RTTIType.Building;
        }
    }
}
