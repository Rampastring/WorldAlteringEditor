using TSMapEditor.Models.ArtConfig;

namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'VehicleType', but let's respect the original game's naming.
    /// </summary>
    public class UnitType : TechnoType, IArtConfigContainer
    {
        public UnitType(string iniName) : base(iniName)
        {
        }

        public VehicleArtConfig ArtConfig { get; private set; } = new VehicleArtConfig();
        public IArtConfig GetArtConfig() => ArtConfig;

        public override RTTIType WhatAmI() => RTTIType.Unit;
    }
}
