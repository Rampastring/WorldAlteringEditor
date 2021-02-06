using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A terrain object. For example, a tree.
    /// </summary>
    public class TerrainObject : GameObject
    {
        public TerrainObject(TerrainType terrainType, Point2D position)
        {
            TerrainType = terrainType;
            Position = position;
        }

        public override RTTIType WhatAmI() => RTTIType.Terrain;

        public TerrainType TerrainType { get; private set; }

        public override int GetYDrawOffset()
        {
            return base.GetYDrawOffset();
        }
    }
}
