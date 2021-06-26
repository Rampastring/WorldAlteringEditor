using TSMapEditor.Models.ArtConfig;

namespace TSMapEditor.Models
{
    public class BuildingType : TechnoType, IArtConfigContainer
    {
        public BuildingType(string iniName) : base(iniName)
        {
        }

        public int Upgrades { get; set; }
        public BuildingArtConfig ArtConfig { get; set; } = new BuildingArtConfig();
        public IArtConfig GetArtConfig() => ArtConfig;

        public override RTTIType WhatAmI() => RTTIType.BuildingType;
    }
}
