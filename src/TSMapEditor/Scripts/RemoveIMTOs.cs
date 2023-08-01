using System.Collections.Generic;
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
