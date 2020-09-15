using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class AircraftType : TechnoType
    {
        public AircraftType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.AircraftType;
    }
}
