using Rampastring.Tools;
using TSMapEditor.INI;

namespace TSMapEditor.CCEngine
{
    public class TileSet : INIDefineable
    {
        public int Index { get; set; }
        public string SetName { get; set; }
        public string FileName { get; set; }
        public int TilesInSet { get; set; }
        public bool Morphable { get; set; }
        public int MarbleMadness { get; set; } = -1;
        public int NonMarbleMadness { get; set; } = -1;
        public bool AllowTiberium { get; set; }
        public bool AllowToPlace { get; set; } = true;

        public int StartTileIndex { get; set; }

        /// <summary>
        /// The actual amount of tiles successfully loaded for this tile set.
        /// </summary>
        public int LoadedTileCount { get; set; }

        public void Read(IniSection iniSection)
        {
            SetPropertiesFromSection(iniSection);
        }
    }
}
