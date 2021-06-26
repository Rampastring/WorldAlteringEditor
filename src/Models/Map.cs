using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using TSMapEditor.GameMath;
using TSMapEditor.Initialization;

namespace TSMapEditor.Models
{
    public class Map : IMap
    {
        public IniFile LoadedINI { get; set; }

        public Rules Rules { get; private set; }

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

        public void InitNew(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni)
        {
            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);
            LoadedINI = new IniFile();
            Rules.InitFromINI(LoadedINI, initializer);
            SetTileData(null);
        }

        public void LoadExisting(IniFile rulesIni, IniFile firestormIni, IniFile artIni, IniFile artFirestormIni, IniFile mapIni)
        {
            Initialize(rulesIni, firestormIni, artIni, artFirestormIni);

            LoadedINI = mapIni ?? throw new ArgumentNullException(nameof(mapIni));
            Rules.InitFromINI(mapIni, initializer);

            MapLoader.ReadBasicSection(this, mapIni);
            MapLoader.ReadMapSection(this, mapIni);
            MapLoader.ReadIsoMapPack(this, mapIni);
            MapLoader.ReadOverlays(this, mapIni);
            MapLoader.ReadTerrainObjects(this, mapIni);

            MapLoader.ReadWaypoints(this, mapIni);
            MapLoader.ReadTaskForces(this, mapIni);
            MapLoader.ReadTriggers(this, mapIni);
            MapLoader.ReadTags(this, mapIni);
            MapLoader.ReadScripts(this, mapIni);
            MapLoader.ReadTeamTypes(this, mapIni);

            MapLoader.ReadBuildings(this, mapIni);
            MapLoader.ReadAircraft(this, mapIni);
            MapLoader.ReadUnits(this, mapIni);
            MapLoader.ReadInfantry(this, mapIni);
        }

        public void Write(string path)
        {
            MapWriter.WriteMapSection(this, LoadedINI);
            MapWriter.WriteBasicSection(this, LoadedINI);
            MapWriter.WriteIsoMapPack5(this, LoadedINI);

            MapWriter.WriteAircraft(this, LoadedINI);
            MapWriter.WriteUnits(this, LoadedINI);
            MapWriter.WriteInfantry(this, LoadedINI);

            LoadedINI.WriteIniFile(LoadedINI.FileName.Substring(0, LoadedINI.FileName.Length - 4) + "_test.map");
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

            house = new House() { ININame = houseName };
            StandardHouses.Add(house);
            return house;
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

        public void AddScript(Script script)
        {
            Scripts.Add(script);
        }

        public void AddTeamType(TeamType teamType)
        {
            TeamTypes.Add(teamType);
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
