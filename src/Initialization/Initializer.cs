using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                { typeof(TerrainType), InitTerrainTypeArt }
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

            var tiles = new List<IsoMapPack5Tile>(uncompressedData.Count % IsoMapPack5Tile.Size);
            position = 0;
            while (position < uncompressedData.Count - IsoMapPack5Tile.Size)
            {
                // This could be optimized by not creating the tile at all if its tile index is 0xFFFF
                var isoMapPack5Tile = new IsoMapPack5Tile(uncompressedData.GetRange(position, IsoMapPack5Tile.Size).ToArray());
                if (isoMapPack5Tile.TileIndex != ushort.MaxValue)
                {
                    tiles.Add(isoMapPack5Tile);
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

        public void InitArt(IniFile iniFile)
        {

        }

        private static void InitBuildingType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var buildingType = (BuildingType)obj;
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
