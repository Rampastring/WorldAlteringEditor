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
