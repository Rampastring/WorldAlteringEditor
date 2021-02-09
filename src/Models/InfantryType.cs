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
