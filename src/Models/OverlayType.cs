using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models.ArtData;
using TSMapEditor.Models.Enums;

namespace TSMapEditor.Models
{
    public class OverlayType : GameObjectType
    {
        public OverlayType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.OverlayType;

        // We might not need all of these properties at least immediately,
        // but I tried listing them all for possible future convenience

        public string Name { get; set; }
        public LandType Land { get; set; }
        public string Image { get; set; }
        public OverlayArtConfig ArtConfig { get; } = new OverlayArtConfig();
        public bool WaterBound { get; set; }
        public bool Wall { get; set; }
        public bool RadarInvisible { get; set; }
        public bool Crushable { get; set; }
        public bool DrawFlat { get; set; } = true;
        public bool NoUseTileLandType { get; set; }
        public bool IsARock { get; set; }
        public bool Tiberium { get; set; }
        public bool IsVeins { get; set; }
        public bool IsVeinholeMonster { get; set; }
    }
}
