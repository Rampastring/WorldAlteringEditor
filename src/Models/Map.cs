using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TSMapEditor.GameMath;
using TSMapEditor.Initialization;

namespace TSMapEditor.Models
{
    public class Map : IMap
    {
        public IniFile LoadedINI { get; set; }

        public Rules Rules { get; private set; }
        public EditorConfig EditorConfig { get; private set; }

        public BasicSection Basic { get; private set; } = new BasicSection();

        public MapTile[][] Tiles { get; private set; } = new MapTile[600][]; // for now
        public MapTile GetTile(int x, int y)
        {
            if (y < 0 || y >= Tiles.Length)
                return null;

            if (x < 0 || x >= Tiles[y].Length)
                return null;

            return Tiles[y][x];
        }

        public MapTile GetTile(Point2D cellCoords) => GetTile(cellCoords.X, cellCoords.Y);

        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();
        public List<Infantry> Infantry { get; } = new List<Infantry>();
        public List<Unit> Units { get; } = new List<Unit>();
        public List<Structure> Structures { get; } = new List<Structure>();

        /// <summary>
        /// The list of standard houses loaded from Rules.ini.
        /// Relevant when the map itself has no houses specified.
        /// New houses might be added to this list if the map has
        /// objects whose owner does not exist in the map's list of houses
        /// or in the Rules.ini standard house list.
        /// </summary>
        public List<House> StandardHouses { get; set; }
        public List<House> Houses { get; } = new List<House>();
        public List<TerrainObject> TerrainObjects { get; } = new List<TerrainObject>();
        public List<Waypoint> Waypoints { get; } = new List<Waypoint>();

        public List<TaskForce> TaskForces { get; } = new List<TaskForce>();
        public List<Trigger> Triggers { get; } = new List<Trigger>();
        public List<Tag> Tags { get; } = new List<Tag>();
        public List<CellTag> CellTags { get; } = new List<CellTag>();
        public List<Script> Scripts { get; } = new List<Script>();
        public List<TeamType> TeamTypes { get; } = new List<TeamType>();

        public Point2D Size { get; set; }
        public Rectangle LocalSize { get; set; }
        public string Theater { get; set; }

        private readonly Initializer initializer;

        public Map()
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new MapTile[600];
            }

            initializer = new Initializer(this);
        }

        private void InitEditorConfig()
        {
            EditorConfig = new EditorConfig();
            EditorConfig.ReadOverlayCollections(Rules);
            EditorConfig.ReadBrushSizes();
        }

        public void InitNew(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni)
        {
            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);
            LoadedINI = new IniFile();
            Rules.InitFromINI(LoadedINI, initializer);
            InitEditorConfig();
            SetTileData(null);
        }

        public void LoadExisting(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni, IniFile mapIni)
        {
            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);

            LoadedINI = mapIni ?? throw new ArgumentNullException(nameof(mapIni));
            Rules.InitFromINI(mapIni, initializer);
            InitEditorConfig();

            MapLoader.ReadBasicSection(this, mapIni);
            MapLoader.ReadMapSection(this, mapIni);
            MapLoader.ReadIsoMapPack(this, mapIni);

            MapLoader.ReadHouses(this, mapIni);

            MapLoader.ReadOverlays(this, mapIni);
            MapLoader.ReadTerrainObjects(this, mapIni);

            MapLoader.ReadWaypoints(this, mapIni);
            MapLoader.ReadTaskForces(this, mapIni);
            MapLoader.ReadTriggers(this, mapIni);
            MapLoader.ReadTags(this, mapIni);
            MapLoader.ReadCellTags(this, mapIni);
            MapLoader.ReadScripts(this, mapIni);
            MapLoader.ReadTeamTypes(this, mapIni);

            MapLoader.ReadBuildings(this, mapIni);
            MapLoader.ReadAircraft(this, mapIni);
            MapLoader.ReadUnits(this, mapIni);
            MapLoader.ReadInfantry(this, mapIni);
        }

        public void Write(string path)
        {
            LoadedINI.Comment = "Written by DTA Scenario Editor\r\n; all comments have been truncated\r\n; www.moddb.com/members/Rampastring\r\n; github.com/Rampastring";

            MapWriter.WriteMapSection(this, LoadedINI);
            MapWriter.WriteBasicSection(this, LoadedINI);
            MapWriter.WriteIsoMapPack5(this, LoadedINI);

            MapWriter.WriteHouses(this, LoadedINI);

            MapWriter.WriteOverlays(this, LoadedINI);
            MapWriter.WriteTerrainObjects(this, LoadedINI);

            MapWriter.WriteWaypoints(this, LoadedINI);
            MapWriter.WriteTaskForces(this, LoadedINI);
            MapWriter.WriteTriggers(this, LoadedINI);
            MapWriter.WriteTags(this, LoadedINI);
            MapWriter.WriteCellTags(this, LoadedINI);
            MapWriter.WriteScripts(this, LoadedINI);
            MapWriter.WriteTeamTypes(this, LoadedINI);

            MapWriter.WriteAircraft(this, LoadedINI);
            MapWriter.WriteUnits(this, LoadedINI);
            MapWriter.WriteInfantry(this, LoadedINI);
            MapWriter.WriteBuildings(this, LoadedINI);

            //LoadedINI.WriteIniFile(LoadedINI.FileName.Substring(0, LoadedINI.FileName.Length - 4) + "_test.map");
            LoadedINI.WriteIniFile(LoadedINI.FileName);
        }

        /// <summary>
        /// Finds a house with the given name from the map's or the game's house lists.
        /// If no house is found, creates one and adds it to the game's house list.
        /// Returns the house that was found or created.
        /// </summary>
        /// <param name="houseName">The name of the house to find.</param>
        public House FindOrMakeHouse(string houseName)
        {
            var house = Houses.Find(h => h.ININame == houseName);
            if (house != null)
                return house;

            house = StandardHouses.Find(h => h.ININame == houseName);
            if (house != null)
                return house;

            house = new House(houseName);
            StandardHouses.Add(house);
            return house;
        }

        /// <summary>
        /// Finds a house with the given name from the map's or the game's house lists.
        /// Returns null if no house is found.
        /// </summary>
        /// <param name="houseName">The name of the house to find.</param>
        public House FindHouse(string houseName)
        {
            var house = Houses.Find(h => h.ININame == houseName);
            if (house != null)
                return house;

            return StandardHouses.Find(h => h.ININame == houseName);
        }

        public void SetTileData(List<MapTile> tiles)
        {
            if (tiles != null)
            {
                foreach (var tile in tiles)
                {
                    Tiles[tile.Y][tile.X] = tile;
                }
            }

            // Check for uninitialized tiles within the map bounds
            // Begin from the top-left corner and proceed row by row
            int ox = 1;
            int oy = Size.X;
            while (ox <= Size.Y)
            {
                int tx = ox;
                int ty = oy;
                while (tx < Size.X + ox)
                {
                    if (Tiles[ty][tx] == null)
                    {
                        Tiles[ty][tx] = new MapTile() { X = (short)tx, Y = (short)ty };
                    }

                    if (tx < Size.X + ox - 1 && Tiles[ty][tx + 1] == null)
                    {
                        Tiles[ty][tx + 1] = new MapTile() { X = (short)(tx + 1), Y = (short)ty };
                    }

                    tx++;
                    ty--;
                }

                ox++;
                oy++;
            }
        }

        public void AddWaypoint(Waypoint waypoint)
        {
            Waypoints.Add(waypoint);
            GetTile(waypoint.Position.X, waypoint.Position.Y).Waypoint = waypoint;
        }

        public void AddTaskForce(TaskForce taskForce)
        {
            TaskForces.Add(taskForce);
        }

        public void AddTrigger(Trigger trigger)
        {
            Triggers.Add(trigger);
        }

        public void AddTag(Tag tag)
        {
            Tags.Add(tag);
        }

        public void AddCellTag(CellTag cellTag)
        {
            var tile = GetTile(cellTag.Position.X, cellTag.Position.Y);
            if (tile.CellTag != null)
            {
                Logger.Log("Tile already has a celltag, skipping placing of celltag at " + cellTag.Position);
                return;
            }

            CellTags.Add(cellTag);
            GetTile(cellTag.Position.X, cellTag.Position.Y).CellTag = cellTag;
        }

        public void AddScript(Script script)
        {
            Scripts.Add(script);
        }

        public void AddTeamType(TeamType teamType)
        {
            TeamTypes.Add(teamType);
        }

        public void PlaceUnit(Unit unit)
        {
            var cell = GetTile(unit.Position);
            if (cell.Vehicle != null)
                throw new InvalidOperationException("Cannot place a vehicle on a cell that already has a vehicle!");

            cell.Vehicle = unit;
            Units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            var cell = GetTile(unit.Position);
            cell.Vehicle = null;
            Units.Remove(unit);
        }

        public void PlaceInfantry(Infantry infantry)
        {
            var cell = GetTile(infantry.Position);
            if (cell.Infantry[(int)infantry.SubCell] != null)
                throw new InvalidOperationException("Cannot place infantry on an occupied sub-cell spot!");

            cell.Infantry[(int)infantry.SubCell] = infantry;
            Infantry.Add(infantry);
        }

        public void RemoveInfantry(Infantry infantry)
        {
            var cell = GetTile(infantry.Position);
            cell.Infantry[(int)infantry.SubCell] = null;
            Infantry.Remove(infantry);
        }

        public void PlaceAircraft(Aircraft aircraft)
        {
            var cell = GetTile(aircraft.Position);
            if (cell.Aircraft != null)
                throw new InvalidOperationException("Cannot place an aircraft on a cell that already has an aircraft!");

            cell.Aircraft = aircraft;
            Aircraft.Add(aircraft);
        }

        public void RemoveAircraft(Aircraft aircraft)
        {
            var cell = GetTile(aircraft.Position);
            cell.Aircraft = null;
            Aircraft.Remove(aircraft);
        }

        public void AddTerrainObject(TerrainObject terrainObject)
        {
            var cell = GetTile(terrainObject.Position);
            if (cell.TerrainObject != null)
                throw new InvalidOperationException("Cannot place a terrain object on a cell that already has a terrain object!");

            cell.TerrainObject = terrainObject;
            TerrainObjects.Add(terrainObject);
        }

        public void RemoveTerrainObject(Point2D cellCoords)
        {
            var cell = GetTile(cellCoords);
            TerrainObjects.Remove(cell.TerrainObject);
            cell.TerrainObject = null;
        }

        public int GetOverlayFrameIndex(Point2D cellCoords)
        {
            var cell = GetTile(cellCoords);
            if (cell.Overlay == null)
                return Constants.NO_OVERLAY;

            if (!cell.Overlay.OverlayType.Tiberium)
                return cell.Overlay.FrameIndex;

            // Smooth out tiberium

            int[] frameIndexesForEachAdjacentTiberiumCell = { 0, 1, 3, 4, 6, 7, 8, 10, 11 };
            int adjTiberiumCount = 0;

            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (y == 0 && x == 0)
                        continue;

                    var otherTile = GetTile(cellCoords + new Point2D(x, y));
                    if (otherTile != null && otherTile.Overlay != null)
                    {
                        if (otherTile.Overlay.OverlayType.Tiberium)
                            adjTiberiumCount++;
                    }
                }
            }

            return frameIndexesForEachAdjacentTiberiumCell[adjTiberiumCount];
        }

        public void DoForAllValidTiles(Action<MapTile> action)
        {
            for (int y = 0; y < Tiles.Length; y++)
            {
                for (int x = 0; x < Tiles[y].Length; x++)
                {
                    MapTile tile = Tiles[y][x];

                    if (tile == null)
                        continue;

                    action(tile);
                }
            }
        }

        // public void StartNew(IniFile rulesIni, IniFile firestormIni, TheaterType theaterType, Point2D size)
        // {
        //     Initialize(rulesIni, firestormIni);
        //     LoadedINI = new IniFile();
        // }

        public void Initialize(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni)
        {
            if (rulesIni == null)
                throw new ArgumentNullException(nameof(rulesIni));

            Rules = new Rules();
            Rules.InitFromINI(rulesIni, initializer);

            StandardHouses = Rules.GetStandardHouses(rulesIni);

            if (firestormIni != null)
            {
                Rules.InitFromINI(firestormIni, initializer);
            }

            Rules.InitArt(artIni, initializer);

            if (artFirestormIni != null)
            {
                Rules.InitArt(artFirestormIni, initializer);
            }
        }
    }
}
