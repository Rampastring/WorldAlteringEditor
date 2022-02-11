using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Initialization;
using TSMapEditor.Rendering;

namespace TSMapEditor.Models
{
    public class Map : IMap
    {
        public event EventHandler HousesChanged;

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
        public MapTile GetTileOrFail(Point2D cellCoords) => GetTile(cellCoords.X, cellCoords.Y) ?? throw new InvalidOperationException("Invalid cell coords: " + cellCoords);
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
        public List<House> GetHouses() => Houses.Count > 0 ? Houses : StandardHouses;

        public List<TerrainObject> TerrainObjects { get; } = new List<TerrainObject>();
        public List<Waypoint> Waypoints { get; } = new List<Waypoint>();

        public List<TaskForce> TaskForces { get; } = new List<TaskForce>();
        public List<Trigger> Triggers { get; } = new List<Trigger>();
        public List<Tag> Tags { get; } = new List<Tag>();
        public List<CellTag> CellTags { get; } = new List<CellTag>();
        public List<Script> Scripts { get; } = new List<Script>();
        public List<TeamType> TeamTypes { get; } = new List<TeamType>();
        public List<LocalVariable> LocalVariables { get; } = new List<LocalVariable>();

        public Point2D Size { get; set; }
        public Rectangle LocalSize { get; set; }
        public string TheaterName { get; set; }
        public ITheater TheaterInstance { get; set; }

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
            EditorConfig.Init(Rules);
        }

        public void InitNew(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni, string theaterName, Point2D size)
        {
            const int marginY = 6;
            const int marginX = 4;

            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);
            LoadedINI = new IniFile();
            var baseMap = new IniFile(Environment.CurrentDirectory + "/Config/BaseMap.ini");
            baseMap.FileName = string.Empty;
            baseMap.SetStringValue("Map", "Theater", theaterName);
            baseMap.SetStringValue("Map", "Size", $"0,0,{size.X},{size.Y}");
            baseMap.SetStringValue("Map", "LocalSize", $"{marginX},{marginY},{size.X - (marginX * 2)},{size.Y - (marginY * 2)}");
            LoadExisting(rulesIni, firestormIni, artIni, artFirestormIni, baseMap);
            SetTileData(null);
        }

        public void LoadExisting(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni, IniFile mapIni)
        {
            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);

            LoadedINI = mapIni ?? throw new ArgumentNullException(nameof(mapIni));
            Rules.InitFromINI(mapIni, initializer, true);
            InitEditorConfig();

            MapLoader.ReadBasicSection(this, mapIni);
            MapLoader.ReadMapSection(this, mapIni);
            MapLoader.ReadIsoMapPack(this, mapIni);

            MapLoader.ReadHouses(this, mapIni);

            MapLoader.ReadSmudges(this, mapIni);
            MapLoader.ReadOverlays(this, mapIni);
            MapLoader.ReadTerrainObjects(this, mapIni);

            MapLoader.ReadWaypoints(this, mapIni);
            MapLoader.ReadTaskForces(this, mapIni);
            MapLoader.ReadTriggers(this, mapIni);
            MapLoader.ReadTags(this, mapIni);
            MapLoader.ReadCellTags(this, mapIni);
            MapLoader.ReadScripts(this, mapIni);
            MapLoader.ReadTeamTypes(this, mapIni);
            MapLoader.ReadLocalVariables(this, mapIni);

            MapLoader.ReadBuildings(this, mapIni);
            MapLoader.ReadAircraft(this, mapIni);
            MapLoader.ReadUnits(this, mapIni);
            MapLoader.ReadInfantry(this, mapIni);
        }

        public void Write()
        {
            LoadedINI.Comment = "Written by DTA Scenario Editor\r\n; all comments have been truncated\r\n; www.moddb.com/members/Rampastring\r\n; github.com/Rampastring";

            MapWriter.WriteMapSection(this, LoadedINI);
            MapWriter.WriteBasicSection(this, LoadedINI);
            MapWriter.WriteIsoMapPack5(this, LoadedINI);

            MapWriter.WriteHouses(this, LoadedINI);

            MapWriter.WriteSmudges(this, LoadedINI);
            MapWriter.WriteOverlays(this, LoadedINI);
            MapWriter.WriteTerrainObjects(this, LoadedINI);

            MapWriter.WriteWaypoints(this, LoadedINI);
            MapWriter.WriteTaskForces(this, LoadedINI);
            MapWriter.WriteTriggers(this, LoadedINI);
            MapWriter.WriteTags(this, LoadedINI);
            MapWriter.WriteCellTags(this, LoadedINI);
            MapWriter.WriteScripts(this, LoadedINI);
            MapWriter.WriteTeamTypes(this, LoadedINI);
            MapWriter.WriteLocalVariables(this, LoadedINI);

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

        public void PlaceTerrainTileAt(ITileImage tile, Point2D cellCoords)
        {
            for (int i = 0; i < tile.SubTileCount; i++)
            {
                var subTile = tile.GetSubTile(i);
                if (subTile.TmpImage == null)
                    continue;

                Point2D offset = tile.GetSubTileCoordOffset(i).Value;

                var mapTile = GetTile(cellCoords + offset);
                if (mapTile == null)
                    continue;

                mapTile.TileImage = null;
                mapTile.TileIndex = tile.TileID;
                mapTile.SubTileIndex = (byte)i;
            }
        }

        public void AddWaypoint(Waypoint waypoint)
        {
            Waypoints.Add(waypoint);
            var cell = GetTile(waypoint.Position.X, waypoint.Position.Y);
            if (cell.Waypoint != null)
                throw new InvalidOperationException("Cannot add waypoint to a cell that already has a waypoint!");

            cell.Waypoint = waypoint;
        }

        public void RemoveWaypoint(Waypoint waypoint)
        {
            var tile = GetTile(waypoint.Position);
            if (tile.Waypoint == waypoint)
            {
                Waypoints.Remove(waypoint);
                tile.Waypoint = null;
            }
        }

        public void RemoveWaypointFrom(Point2D cellCoords)
        {
            var tile = GetTile(cellCoords);
            if (tile.Waypoint != null)
            {
                Waypoints.Remove(tile.Waypoint);
                tile.Waypoint = null;
            }
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
            var tile = GetTile(cellTag.Position);
            if (tile.CellTag != null)
            {
                Logger.Log("Tile already has a celltag, skipping placing of celltag at " + cellTag.Position);
                return;
            }

            CellTags.Add(cellTag);
            tile.CellTag = cellTag;
        }

        public void RemoveCellTagFrom(Point2D cellCoords)
        {
            var tile = GetTile(cellCoords);
            if (tile.CellTag != null)
            {
                CellTags.Remove(tile.CellTag);
                tile.CellTag = null;
            }
        }

        public void AddScript(Script script)
        {
            Scripts.Add(script);
        }

        public void AddTeamType(TeamType teamType)
        {
            TeamTypes.Add(teamType);
        }

        public void AddHouses(List<House> houses)
        {
            if (houses.Count > 0)
            {
                Houses.AddRange(houses);
                HousesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AddHouse(House house)
        {
            Houses.Add(house);
            HousesChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool DeleteHouse(House house)
        {
            if (Houses.Remove(house))
            {
                HousesChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        public void PlaceBuilding(Structure structure)
        {
            structure.ObjectType.ArtConfig.DoForFoundationCoords(offset =>
            {
                var cell = GetTile(structure.Position + offset);
                if (cell == null)
                    return;

                if (cell.Structure != null)
                    throw new InvalidOperationException("Cannot place a structure on a cell that already has a structure!");

                cell.Structure = structure;
            });

            if (structure.ObjectType.ArtConfig.FoundationX == 0 && structure.ObjectType.ArtConfig.FoundationY == 0)
            {
                GetTile(structure.Position).Structure = structure;
            }
            
            Structures.Add(structure);
        }

        public void RemoveBuilding(Structure structure)
        {
            structure.ObjectType.ArtConfig.DoForFoundationCoords(offset =>
            {
                var cell = GetTile(structure.Position + offset);
                if (cell == null)
                    return;

                if (cell.Structure == structure)
                    cell.Structure = null;
            });

            if (structure.ObjectType.ArtConfig.FoundationX == 0 && structure.ObjectType.ArtConfig.FoundationY == 0)
            {
                GetTile(structure.Position).Structure = null;
            }

            Structures.Remove(structure);
        }

        public void MoveBuilding(Structure structure, Point2D newCoords)
        {
            RemoveBuilding(structure);
            structure.Position = newCoords;
            PlaceBuilding(structure);
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

        public void MoveUnit(Unit unit, Point2D newCoords)
        {
            RemoveUnit(unit);
            unit.Position = newCoords;
            PlaceUnit(unit);
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

        public void MoveInfantry(Infantry infantry, Point2D newCoords)
        {
            var newCell = GetTile(newCoords);
            SubCell freeSubCell = newCell.GetFreeSubCellSpot();
            RemoveInfantry(infantry);
            infantry.Position = newCoords;
            infantry.SubCell = freeSubCell;
            PlaceInfantry(infantry);
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

        public void MoveAircraft(Aircraft aircraft, Point2D newCoords)
        {
            RemoveAircraft(aircraft);
            aircraft.Position = newCoords;
            PlaceAircraft(aircraft);
        }

        public void AddTerrainObject(TerrainObject terrainObject)
        {
            var cell = GetTile(terrainObject.Position);
            if (cell.TerrainObject != null)
                throw new InvalidOperationException("Cannot place a terrain object on a cell that already has a terrain object!");

            cell.TerrainObject = terrainObject;
            TerrainObjects.Add(terrainObject);
        }

        public void RemoveTerrainObject(TerrainObject terrainObject)
        {
            RemoveTerrainObject(terrainObject.Position);
        }

        public void RemoveTerrainObject(Point2D cellCoords)
        {
            var cell = GetTile(cellCoords);
            TerrainObjects.Remove(cell.TerrainObject);
            cell.TerrainObject = null;
        }

        public void MoveTerrainObject(TerrainObject terrainObject, Point2D newCoords)
        {
            RemoveTerrainObject(terrainObject.Position);
            terrainObject.Position = newCoords;
            AddTerrainObject(terrainObject);
        }

        public void MoveWaypoint(Waypoint waypoint, Point2D newCoords)
        {
            RemoveWaypoint(waypoint);
            waypoint.Position = newCoords;
            AddWaypoint(waypoint);
        }

        /// <summary>
        /// Determines whether an object can be moved to a specific location.
        /// </summary>
        /// <param name="gameObject">The object to move.</param>
        /// <param name="newCoords">The new coordinates of the object.</param>
        /// <returns>True if the object can be moved, otherwise false.</returns>
        public bool CanMoveObject(IMovable movable, Point2D newCoords)
        {
            if (movable.WhatAmI() == RTTIType.Building)
            {
                bool canPlace = true;

                ((Structure)movable).ObjectType.ArtConfig.DoForFoundationCoords(offset =>
                {
                    MapTile foundationCell = GetTile(newCoords + offset);
                    if (foundationCell == null)
                        return;

                    if (foundationCell.Structure != null && foundationCell.Structure != movable)
                        canPlace = false;
                });

                if (!canPlace)
                    return false;
            }

            MapTile cell = GetTile(newCoords);
            if (movable.WhatAmI() == RTTIType.Waypoint)
                return cell.Waypoint == null;

            return cell.CanAddObject((GameObject)movable);
        }

        public void DeleteObjectFromCell(Point2D cellCoords)
        {
            var tile = GetTile(cellCoords.X, cellCoords.Y);
            if (tile == null)
                return;

            for (int i = 0; i < tile.Infantry.Length; i++)
            {
                if (tile.Infantry[i] != null)
                {
                    RemoveInfantry(tile.Infantry[i]);
                    return;
                }
            }

            if (tile.Aircraft != null)
            {
                RemoveAircraft(tile.Aircraft);
                return;
            }

            if (tile.Vehicle != null)
            {
                RemoveUnit(tile.Vehicle);
                return;
            }

            if (tile.Structure != null)
            {
                RemoveBuilding(tile.Structure);
                return;
            }

            if (tile.TerrainObject != null)
            {
                RemoveTerrainObject(tile.CoordsToPoint());
                return;
            }

            if (tile.CellTag != null)
            {
                RemoveCellTagFrom(tile.CoordsToPoint());
                return;
            }

            if (tile.Waypoint != null)
            {
                RemoveWaypoint(tile.Waypoint);
                return;
            }
        }

        public int GetOverlayFrameIndex(Point2D cellCoords)
        {
            var cell = GetTile(cellCoords);
            if (cell.Overlay == null || cell.Overlay.OverlayType == null)
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

        public int GetAutoLATIndex(MapTile mapTile, int baseLATTileSetIndex, int transitionLATTileSetIndex)
        {
            foreach (var autoLatData in AutoLATType.AutoLATData)
            {
                if (TransitionArrayDataMatches(autoLatData.TransitionMatchArray, mapTile, baseLATTileSetIndex, transitionLATTileSetIndex))
                {
                    return autoLatData.TransitionTypeIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Convenience structure for <see cref="TransitionArrayDataMatches(int[], MapTile, int, int)"/>.
        /// </summary>
        struct NearbyTileData
        {
            public int XOffset;
            public int YOffset;
            public int DirectionIndex;

            public NearbyTileData(int xOffset, int yOffset, int directionIndex)
            {
                XOffset = xOffset;
                YOffset = yOffset;
                DirectionIndex = directionIndex;
            }
        }

        /// <summary>
        /// Checks if specific transition data matches for a tile.
        /// If it does, then the tile should use the LAT transition index related to the data.
        /// </summary>
        private bool TransitionArrayDataMatches(int[] transitionData, MapTile mapTile, int desiredTileSetId1, int desiredTileSetId2)
        {
            var nearbyTiles = new NearbyTileData[]
            {
                new NearbyTileData(0, -1, AutoLATType.NE_INDEX),
                new NearbyTileData(-1, 0, AutoLATType.NW_INDEX),
                new NearbyTileData(0, 0, AutoLATType.CENTER_INDEX),
                new NearbyTileData(1, 0, AutoLATType.SE_INDEX),
                new NearbyTileData(0, 1, AutoLATType.SW_INDEX)
            };

            foreach (var nearbyTile in nearbyTiles)
            {
                if (!TileSetMatchesExpected(mapTile.X + nearbyTile.XOffset, mapTile.Y + nearbyTile.YOffset,
                    transitionData, nearbyTile.DirectionIndex, desiredTileSetId1, desiredTileSetId2))
                {
                    return false;
                }
            }

            return true;
        }

        private bool TileSetMatchesExpected(int x, int y, int[] transitionData, int transitionDataIndex, int desiredTileSetId1, int desiredTileSetId2)
        {
            var tile = GetTile(x, y);

            if (tile == null)
                return true;

            bool shouldMatch = transitionData[transitionDataIndex] > 0;

            int tileSetId = TheaterInstance.GetTileSetId(tile.TileIndex);
            if (shouldMatch && (tileSetId != desiredTileSetId1 && tileSetId != desiredTileSetId2))
                return false;

            if (!shouldMatch && (tileSetId == desiredTileSetId1 || tileSetId == desiredTileSetId2))
                return false;

            return true;
        }

        /// <summary>
        /// Generates an unique internal ID.
        /// Used for new TaskForces, Scripts, TeamTypes and Triggers.
        /// </summary>
        /// <returns></returns>
        public string GetNewUniqueInternalId()
        {
            int id = 1000000;
            string idString = string.Empty;

            while (true)
            {
                idString = "0" + id.ToString(CultureInfo.InvariantCulture);

                if (TaskForces.Exists(tf => tf.ININame == idString) || 
                    Scripts.Exists(s => s.ININame == idString) || 
                    TeamTypes.Exists(tt => tt.ININame == idString) ||
                    Triggers.Exists(t => t.ID == idString) || Tags.Exists(t => t.ID == idString) ||
                    LoadedINI.SectionExists(idString))
                {
                    id++;
                    continue;
                }

                break;
            }

            return idString;
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

            var editorRulesIni = new IniFile(Environment.CurrentDirectory + "/Config/EditorRules.ini");

            StandardHouses = Rules.GetStandardHouses(editorRulesIni);
            if (StandardHouses.Count == 0)
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

            // Load impassable cell information for terrain types
            var impassableTerrainObjectsIni = new IniFile(Environment.CurrentDirectory + "/Config/TerrainTypeImpassability.ini");

            Rules.TerrainTypes.ForEach(tt =>
            {
                string value = impassableTerrainObjectsIni.GetStringValue(tt.ININame, "ImpassableCells", null);
                if (string.IsNullOrWhiteSpace(value))
                    return;

                string[] cellInfos = value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cellInfo in cellInfos)
                {
                    Point2D point = Point2D.FromString(cellInfo);
                    if (tt.ImpassableCells == null)
                        tt.ImpassableCells = new List<Point2D>(2);

                    tt.ImpassableCells.Add(point);
                }
            });
        }
    }
}
