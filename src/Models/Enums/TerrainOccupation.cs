using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models.Enums
{
    [Flags]
    public enum TerrainOccupation
    {
        None = 0,
        East = 1,
        West = 2,
        South = 4
    }
}
