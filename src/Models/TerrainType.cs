using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models.Enums;

namespace TSMapEditor.Models
{
    public class TerrainType : GameObjectType
    {
        public TerrainType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.TerrainType;

        public int Index { get; set; }

        public string Name { get; set; }
        public string FSName { get; set; }
        public TerrainOccupation TemperateOccupationBits { get; set; }
        public TerrainOccupation SnowOccupationBits { get; set; }

        // These ones below don't exist in TS or FinalSun,
        // but they'd be useful in DTA at least
        public bool AvailableOnTemperate { get; set; }
        public bool AvailableOnSnow { get; set; }
    }
}
