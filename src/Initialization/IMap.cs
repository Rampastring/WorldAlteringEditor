using System.Collections.Generic;
using Rampastring.Tools;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Initialization
{
    public interface IMap
    {
        MapTile[][] Tiles { get; }
        List<House> StandardHouses { get; }
        List<Aircraft> Aircraft { get; }
        List<House> Houses { get; }
        List<Infantry> Infantry { get; }
        IniFile LoadedINI { get; }
        Rules Rules { get; }
        List<Structure> Structures { get; }
        List<TerrainObject> TerrainObjects { get; }
        List<Unit> Units { get; }
        List<Waypoint> Waypoints { get; }
        Point2D Size { get; set; }
        void SetTileData(List<MapTile> tiles);
        House FindOrMakeHouse(string houseName);
    }
}