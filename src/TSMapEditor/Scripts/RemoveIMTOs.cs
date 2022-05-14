using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models;

namespace TSMapEditor.Scripts
{
    public class RemoveIMTOs
    {
        public static void Perform(Map map)
        {
            var terrainObjectsCopy = new List<TerrainObject>(map.TerrainObjects);

            terrainObjectsCopy.ForEach(tobj =>
            {
                if (tobj.TerrainType.ININame == "IMTO")
                    map.RemoveTerrainObject(tobj);
            });
        }
    }
}
