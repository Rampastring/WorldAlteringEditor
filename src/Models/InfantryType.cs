using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class InfantryType : TechnoType
    {
        public InfantryType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.InfantryType;
    }
}
