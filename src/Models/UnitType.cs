using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class UnitType : TechnoType
    {
        public UnitType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Unit;
    }
}
