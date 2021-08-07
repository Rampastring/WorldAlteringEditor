using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Initialization
{
    public interface IMap
    {
        BasicSection Basic { get; }

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
        List<CellTag> CellTags { get; }
        List<Script> Scripts { get; }
        List<TaskForce> TaskForces { get; }
        List<TeamType> TeamTypes { get; }

        Point2D Size { get; set; }
        Rectangle LocalSize { get; set; }
        string TheaterName { get; set; }

        void SetTileData(List<MapTile> tiles);
        House FindOrMakeHouse(string houseName);
        House FindHouse(string houseName);
        void AddWaypoint(Waypoint waypoint);
        void AddTaskForce(TaskForce taskForce);
        void AddTrigger(Trigger trigger);
        void AddTag(Tag tag);
        void AddCellTag(CellTag cellTag);
        void RemoveCellTagFrom(Point2D cellCoords);
        void AddScript(Script script);
        void AddTeamType(TeamType teamType);

        void PlaceUnit(Unit unit);
        void RemoveUnit(Unit unit);

        void DoForAllValidTiles(Action<MapTile> action);

        void Write(string path);
    }
}