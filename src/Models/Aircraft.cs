using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class Aircraft : Foot<AircraftType>
    {
        public Aircraft(AircraftType objectType) : base(objectType)
        {
        }

        // [Aircraft]
        // INDEX=OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

        public override RTTIType WhatAmI() => RTTIType.Aircraft;
    }
}
