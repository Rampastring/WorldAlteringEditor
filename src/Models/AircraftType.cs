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
