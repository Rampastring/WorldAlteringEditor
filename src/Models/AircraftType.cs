using TSMapEditor.Models.ArtConfig;

namespace TSMapEditor.Models
{
    public class AircraftType : TechnoType, IArtConfigContainer
    {
        public AircraftType(string iniName) : base(iniName)
        {
        }

        public IArtConfig GetArtConfig()
        {
            return null;
        }

        public override RTTIType WhatAmI() => RTTIType.AircraftType;
    }
}
