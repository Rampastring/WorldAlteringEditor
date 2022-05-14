using TSMapEditor.GameMath;

namespace TSMapEditor.Mutations.Classes
{
    public struct OriginalTerrainData
    {
        public OriginalTerrainData(int tileIndex, int subTileIndex, Point2D cellCoords)
        {
            TileIndex = tileIndex;
            SubTileIndex = subTileIndex;
            CellCoords = cellCoords;
        }

        public int TileIndex;
        public int SubTileIndex;
        public Point2D CellCoords;
    }
}
