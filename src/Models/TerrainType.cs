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

        /// <summary>
        /// If set, this terrain type should be drawn 12 pixels above the 
        /// usual drawing point and it should use the unit palette instead
        /// of the terrain palette.
        /// </summary>
        public bool SpawnsTiberium { get; set; }

        /// <summary>
        /// Defined in Art.ini. If set to true,
        /// the art for this terrain type is terrain-specific;
        /// if false, the art is a generic .SHP used for every theater.
        /// </summary>
        public bool Theater { get; set; }

        public string Image { get; set; }

        // These ones below don't exist in TS or FinalSun,
        // but they'd be useful in DTA at least
        public bool AvailableOnTemperate { get; set; }
        public bool AvailableOnSnow { get; set; }
    }
}
