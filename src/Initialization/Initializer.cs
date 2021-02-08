using CNCMaps.FileFormats.Encodings;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Initialization
{
    public class MapLoadException : Exception
    {
        public MapLoadException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Initializes all different object types.
    /// </summary>
    public class Initializer : IInitializer
    {
        private const int MAX_MAP_LENGTH_IN_DIMENSION = 512;
        private const int NO_OVERLAY = 255; // 0xFF

        public Initializer(IMap map)
        {
            this.map = map;
        }

        private readonly IMap map;

        private Dictionary<Type, Action<AbstractObject, IniFile, IniSection>> objectTypeInitializers
            = new Dictionary<Type, Action<AbstractObject, IniFile, IniSection>>()
            {
                { typeof(BuildingType), InitBuildingType },
                { typeof(InfantryType), InitInfantryType },
                { typeof(UnitType), InitUnitType },
                { typeof(AircraftType), InitAircraftType },
                { typeof(OverlayType), InitOverlayType },
                { typeof(TerrainType), InitTerrainType }
            };

        private Dictionary<Type, Action<AbstractObject, IniFile, IniSection>> objectTypeArtInitializers
            = new Dictionary<Type, Action<AbstractObject, IniFile, IniSection>>()
            {
                { typeof(TerrainType), InitTerrainTypeArt },
                { typeof(BuildingType), InitBuildingTypeArt }
            };

        public void ReadObjectTypePropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(obj.ININame);
            if (objectSection == null)
                return;

            obj.ReadPropertiesFromIniSection(objectSection);

            if (objectTypeInitializers.TryGetValue(typeof(T), out var action))
                action(obj, iniFile, objectSection);
        }

        public void ReadObjectTypeArtPropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(obj.ININame);
            if (objectSection == null)
                return;

            if (objectTypeArtInitializers.TryGetValue(typeof(T), out var action))
                action(obj, iniFile, objectSection);
        }

        public void ReadObjectTypeArtPropertiesFromINI<T>(T obj, IniFile iniFile, string sectionName) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(sectionName);
            if (objectSection == null)
                return;

            if (objectTypeArtInitializers.TryGetValue(typeof(T), out var action))
                action(obj, iniFile, objectSection);
        }

        public void ReadMapSection(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Map");
            if (section == null)
                throw new MapLoadException("[Map] does not exist in the loaded file!");

            string size = section.GetStringValue("Size", null);
            if (size == null)
                throw new MapLoadException("Invalid [Map] Size=");
            string[] parts = size.Split(',');
            if (parts.Length != 4)
                throw new MapLoadException("Invalid [Map] Size=");

            int width = int.Parse(parts[2]);
            int height = int.Parse(parts[3]);
            map.Size = new GameMath.Point2D(width, height);
        }

        public void ReadIsoMapPack(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("IsoMapPack5");
            if (section == null)
                return;

            StringBuilder sb = new StringBuilder();
            section.Keys.ForEach(kvp => sb.Append(kvp.Value));

            byte[] compressedData = Convert.FromBase64String(sb.ToString());
            if (compressedData.Length < 4)
                throw new InvalidOperationException("Invalid IsoMapPack5 format");

            Logger.Log("IsoMapPack5 CompressedData length: " + compressedData.Length);

            List<byte> uncompressedData = new List<byte>();

            int position = 0;

            while (position < compressedData.Length)
            {
                ushort inputSize = BitConverter.ToUInt16(compressedData, position);
                ushort outputSize = BitConverter.ToUInt16(compressedData, position + 2);

                Logger.Log("Decoding IsoMapPack5 block: pos: " + position + ", inSize: " + inputSize + ", outSize: " + outputSize);

                if (position + inputSize + 4 > compressedData.Length)
                    throw new InvalidOperationException("Error decoding IsoMapPack5");

                byte[] inData = new byte[inputSize];
                Array.Copy(compressedData, position + 4, inData, 0, inputSize);
                byte[] outData = new byte[outputSize];
                MiniLZO.MiniLZO.Decompress(inData, outData);
                uncompressedData.AddRange(outData);

                position += inputSize + 4;
            }

            // if (uncompressedData.Count % IsoMapPack5Tile.Size != 0)
            //     throw new InvalidOperationException("Decompressed IsoMapPack5 size does not match expected struct size");

            var tiles = new List<MapTile>(uncompressedData.Count % IsoMapPack5Tile.Size);
            position = 0;
            while (position < uncompressedData.Count - IsoMapPack5Tile.Size)
            {
                // This could be optimized by not creating the tile at all if its tile index is 0xFFFF
                var mapTile = new MapTile(uncompressedData.GetRange(position, IsoMapPack5Tile.Size).ToArray());
                if (mapTile.TileIndex != ushort.MaxValue)
                {
                    tiles.Add(mapTile);
                }
                position += IsoMapPack5Tile.Size;
            }

            map.SetTileData(tiles);
        }

        public void ReadTerrainObjects(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Terrain");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                string coords = kvp.Key;
                int yLength = coords.Length - 3;
                int y = Conversions.IntFromString(coords.Substring(0, yLength), -1);
                int x = Conversions.IntFromString(coords.Substring(yLength), -1);
                if (y < 0 || x < 0)
                    continue;

                TerrainType terrainType = map.Rules.TerrainTypes.Find(tt => tt.ININame == kvp.Value);
                if (terrainType == null)
                {
                    Logger.Log($"Skipping loading of terrain type {kvp.Value} because it does not exist in Rules");
                    continue;
                }

                map.TerrainObjects.Add(new TerrainObject(terrainType, new GameMath.Point2D(x, y)));
            }
        }

        public void ReadBuildings(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Structures");
            if (section == null)
                return;

            // [Structures]
            // INDEX=OWNER,ID,HEALTH,X,Y,FACING,TAG,AI_SELLABLE,AI_REBUILDABLE,POWERED_ON,UPGRADES,SPOTLIGHT,UPGRADE_1,UPGRADE_2,UPGRADE_3,AI_REPAIRABLE,NOMINAL

            foreach (var kvp in section.Keys)
            {
                string[] values = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string ownerName = values[0];
                string buildingTypeId = values[1];
                int health = Math.Min(Constants.BuildingHealthMax, Math.Max(0, Conversions.IntFromString(values[2], Constants.BuildingHealthMax)));
                int x = Conversions.IntFromString(values[3], 0);
                int y = Conversions.IntFromString(values[4], 0);
                int facing = Math.Min(Constants.FacingMax, Math.Max(0, Conversions.IntFromString(values[5], Constants.FacingMax)));
                string attachedTag = values[6];
                bool aiSellable = Conversions.BooleanFromString(values[7], true);
                // AI_REBUILDABLE is a leftover
                bool powered = Conversions.BooleanFromString(values[9], true);
                int upgradeCount = Conversions.IntFromString(values[10], 0);
                int spotlight = Conversions.IntFromString(values[11], 0);
                string upgrade1 = values[12];
                string upgrade2 = values[13];
                string upgrade3 = values[14];
                bool aiRepairable = Conversions.BooleanFromString(values[15], false);
                bool nominal = Conversions.BooleanFromString(values[16], false);

                var buildingType = map.Rules.BuildingTypes.Find(bt => bt.ININame == buildingTypeId);
                if (buildingType == null)
                {
                    Logger.Log($"Unable to find building type {buildingTypeId} - skipping it.");
                    continue;
                }

                House owner = map.FindOrMakeHouse(ownerName);
                var building = new Structure(buildingType)
                {
                    HP = health,
                    Position = new GameMath.Point2D(x, y),
                    Facing = (byte)facing,
                    // TODO handle attached tag
                    AISellable = aiSellable,
                    Powered = powered,
                    // TODO handle upgrades
                    Spotlight = (SpotlightType)spotlight,
                    AIRepairable = aiRepairable,
                    Nominal = nominal
                };

                map.Structures.Add(building);
            }
        }

        public void ReadOverlays(IMap map, IniFile mapIni)
        {
            var overlayPackSection = mapIni.GetSection("OverlayPack");
            var overlayDataPackSection = mapIni.GetSection("OverlayDataPack");
            if (overlayPackSection == null || overlayDataPackSection == null)
                return;

            var stringBuilder = new StringBuilder();
            overlayPackSection.Keys.ForEach(kvp => stringBuilder.Append(kvp.Value));
            byte[] compressedData = Convert.FromBase64String(stringBuilder.ToString());
            byte[] uncompressedOverlayPack = new byte[MAX_MAP_LENGTH_IN_DIMENSION * MAX_MAP_LENGTH_IN_DIMENSION];
            Format5.DecodeInto(compressedData, uncompressedOverlayPack, 80);

            stringBuilder.Clear();
            overlayDataPackSection.Keys.ForEach(kvp => stringBuilder.Append(kvp.Value));
            compressedData = Convert.FromBase64String(stringBuilder.ToString());
            byte[] uncompressedOverlayDataPack = new byte[MAX_MAP_LENGTH_IN_DIMENSION * MAX_MAP_LENGTH_IN_DIMENSION];
            Format5.DecodeInto(compressedData, uncompressedOverlayDataPack, 80);

            for (int y = 0; y < map.Tiles.Length; y++)
            {
                for (int x = 0; x < map.Tiles[y].Length; x++)
                {
                    var tile = map.Tiles[y][x];
                    if (tile == null)
                        continue;

                    int overlayDataIndex = (tile.Y * MAX_MAP_LENGTH_IN_DIMENSION) + tile.X;
                    int overlayTypeIndex = uncompressedOverlayPack[overlayDataIndex];
                    if (overlayTypeIndex == NO_OVERLAY)
                        continue;

                    if (overlayTypeIndex >= map.Rules.OverlayTypes.Count)
                    {
                        Logger.Log("Ignoring overlay on tile at " + x + ", " + y + " because it's out of bounds compared to Rules.ini overlay list");
                        continue;
                    }

                    var overlayType = map.Rules.OverlayTypes[overlayTypeIndex];
                    var overlay = new Overlay()
                    {
                        OverlayType = overlayType,
                        FrameIndex = uncompressedOverlayDataPack[overlayDataIndex],
                        Position = new Point2D(tile.X, tile.Y)
                    };
                    tile.Overlay = overlay;
                }
            }
        }

        public void InitArt(IniFile iniFile)
        {

        }

        private static void InitBuildingType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var buildingType = (BuildingType)obj;
        }

        private static void InitBuildingTypeArt(AbstractObject obj, IniFile artIni, IniSection artSection)
        {
            var buildingType = (BuildingType)obj;
            buildingType.ArtData.ReadFromIniSection(artSection);
        }

        private static void InitInfantryType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitUnitType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitAircraftType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitOverlayType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var overlayType = (OverlayType)obj;
            overlayType.Land = (LandType)Enum.Parse(typeof(LandType), section.GetStringValue("Land", LandType.Clear.ToString()));
        }

        private static void InitTerrainType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var terrainType = (TerrainType)obj;
            terrainType.SnowOccupationBits = (TerrainOccupation)section.GetIntValue("SnowOccupationBits", 0);
            terrainType.TemperateOccupationBits = (TerrainOccupation)section.GetIntValue("TemperateOccupationBits", 0);
        }
        
        private static void InitTerrainTypeArt(AbstractObject obj, IniFile artIni, IniSection artSection)
        {
            var terrainType = (TerrainType)obj;
            terrainType.Theater = artSection.GetBooleanValue("Theater", terrainType.Theater);
            terrainType.Image = artSection.GetStringValue("Image", terrainType.Image);
        }
    }
}
