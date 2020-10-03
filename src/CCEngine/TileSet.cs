using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void Read(IniSection iniSection)
        {
            SetPropertiesFromSection(iniSection);
        }
    }
}
