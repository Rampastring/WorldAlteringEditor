using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Initialization;
using TSMapEditor.Models.Enums;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Models
{
    public class Map : IMap
    {
        public IniFile LoadedINI { get; set; }

        public Rules Rules { get; private set; }

        public IsoMapPack5Tile[][] Tiles = new IsoMapPack5Tile[600][]; // for now

        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();
        public List<Infantry> Infantry { get; } = new List<Infantry>();
        public List<Unit> Units { get; } = new List<Unit>();
        public List<Structure> Structures { get; } = new List<Structure>();
        public List<House> Houses { get; } = new List<House>();
        public List<TerrainObject> TerrainObjects { get; } = new List<TerrainObject>();
        public List<Waypoint> Waypoints { get; } = new List<Waypoint>();

        public Point2D Size { get; set; }

        private readonly Initializer initializer;

        public Map()
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i] = new IsoMapPack5Tile[600];
            }

            initializer = new Initializer(this);
        }

        public void LoadExisting(IniFile rulesIni, IniFile firestormIni, IniFile mapIni)
        {
            Initialize(rulesIni, firestormIni);

            LoadedINI = mapIni ?? throw new ArgumentNullException(nameof(mapIni));
            Rules.InitFromINI(mapIni, initializer);
            initializer.ReadMapSection(this, mapIni);
            initializer.ReadIsoMapPack(this, mapIni);
        }

        public void SetTileData(List<IsoMapPack5Tile> tiles)
        {
            foreach (var tile in tiles)
            {
                Tiles[tile.Y][tile.X] = tile;
            }

            // Check for uninitialized tiles within the map bounds
            // Begin from the top-left corner and proceed row by row
            int ox = 1;
            int oy = Size.Y;
            while (ox <= Size.Y)
            {
                int tx = ox;
                int ty = oy;
                while (tx < Size.X + ox)
                {
                    if (Tiles[ty][tx] == null)
                    {
                        Tiles[ty][tx] = new IsoMapPack5Tile() { X = (short)tx, Y = (short)ty };
                    }

                    if (tx < Size.X && Tiles[ty][tx + 1] == null)
                    {
                        Tiles[ty][tx + 1] = new IsoMapPack5Tile() { X = (short)(tx + 1), Y = (short)ty };
                    }

                    tx++;
                    ty--;
                }

                ox++;
                oy++;
            }
        }

        public void StartNew(IniFile rulesIni, IniFile firestormIni, TheaterType theaterType, Point2D size)
        {
            Initialize(rulesIni, firestormIni);
            LoadedINI = new IniFile();
        }

        public void Initialize(IniFile rulesIni, IniFile firestormIni)
        {
            if (rulesIni == null)
                throw new ArgumentNullException(nameof(rulesIni));

            Rules = new Rules();
            Rules.InitFromINI(rulesIni, initializer);

            if (firestormIni != null)
            {
                Rules.InitFromINI(firestormIni, initializer);
            }
        }
    }
}
