using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Models
{
    public class MapTile : IsoMapPack5Tile
    {
        private const int SubCellCount = 5;

        public TerrainObject TerrainObject { get; set; }
        public Structure Structure { get; set; }
        public AbstractObject VehicleOrAircraft { get; set; }
        public Infantry[] Infantry { get; set; } = new Infantry[SubCellCount];

        /// <summary>
        /// A list of objects that graphically overlap with this tile.
        /// When this tile is re-drawn, all the objects in the list should
        /// be re-drawn as well.
        /// </summary>
        public List<AbstractObject> OverlapList { get; set; }
    }
}
