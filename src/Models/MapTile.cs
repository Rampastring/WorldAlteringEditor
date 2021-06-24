using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models.MapFormat;
using TSMapEditor.Rendering;

namespace TSMapEditor.Models
{
    public class MapTile : IsoMapPack5Tile
    {
        private const int SubCellCount = 5;

        public MapTile() { }

        public MapTile(byte[] data) : base(data) { }

        public TileImage TileImage { get; set; }
        public TerrainObject TerrainObject { get; set; }
        public Structure Structure { get; set; }
        public Unit Vehicle { get; set; }
        public Aircraft Aircraft { get; set; }
        public Infantry[] Infantry { get; set; } = new Infantry[SubCellCount];
        public bool AddInfantry(Infantry infantry)
        {
            for (int i = 0; i < Infantry.Length; i++)
            {
                if (Infantry[i] == null)
                {
                    Infantry[i] = infantry;
                    return true;
                }
            }

            return false;
        }

        public TileImage PreviewTileImage { get; set; }
        public int PreviewSubTileIndex { get; set; }

        public Overlay Overlay { get; set; }

        /// <summary>
        /// A list of objects that graphically overlap with this tile.
        /// When this tile is re-drawn, all the objects in the list should
        /// be re-drawn as well.
        /// </summary>
        public List<AbstractObject> OverlapList { get; set; }

        public Point2D CoordsToPoint() => new Point2D(X, Y);
    }
}
