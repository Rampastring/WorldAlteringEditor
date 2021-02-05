using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models.ArtData;

namespace TSMapEditor.Models
{
    public class BuildingType : TechnoType
    {
        public BuildingType(string iniName) : base(iniName)
        {
        }


        public BuildingArtConfig ArtData { get; set; } = new BuildingArtConfig();

        public override RTTIType WhatAmI() => RTTIType.BuildingType;
    }
}
