using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A terrain object. For example, a tree.
    /// </summary>
    public class TerrainObject : GameObject
    {
        public override RTTIType WhatAmI() => RTTIType.Terrain;

        public TerrainType TerrainType { get; private set; }
        
    }
}
