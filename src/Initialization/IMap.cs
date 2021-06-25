using System.Collections.Generic;
using Rampastring.Tools;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Initialization
{
    public interface IMap
    {
        MapTile[][] Tiles { get; }
        MapTile GetTile(int x, int y);
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
        List<Trigger> Triggers { get; }
        List<Tag> Tags { get; }

        Point2D Size { get; set; }

        void SetTileData(List<MapTile> tiles);
        House FindOrMakeHouse(string houseName);
        void AddWaypoint(Waypoint waypoint);
        void AddTaskForce(TaskForce taskForce);
        void AddTrigger(Trigger trigger);
        void AddTag(Tag tag);
    }
}