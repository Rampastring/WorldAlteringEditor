using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.Models;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Initialization
{
    /// <summary>
    /// Contains static methods for writing a map to a INI file.
    /// </summary>
    public static class MapWriter
    {
        private static IniSection FindOrMakeSection(string sectionName, IniFile mapIni)
        {
            var section = mapIni.GetSection(sectionName);
            if (section == null)
            {
                section = new IniSection(sectionName);
                mapIni.AddSection(section);
            }

            return section;
        }

        public static void WriteMapSection(IMap map, IniFile mapIni)
        {
            const string sectionName = "Map";

            var section = FindOrMakeSection(sectionName, mapIni);
            section.SetStringValue("Size", $"0,0,{map.Size.X},{map.Size.Y}");
            section.SetStringValue("Theater", map.Theater);
            section.SetStringValue("LocalSize", $"{map.LocalSize.X},{map.LocalSize.Y},{map.LocalSize.Width},{map.LocalSize.Height}");

            mapIni.AddSection(section);
        }

        public static void WriteBasicSection(IMap map, IniFile mapIni)
        {
            const string sectionName = "Basic";

            var section = FindOrMakeSection(sectionName, mapIni);
            map.Basic.WritePropertiesToIniSection(section);
        }

        public static void WriteIsoMapPack5(IMap map, IniFile mapIni)
        {
            const string sectionName = "IsoMapPack5";
            mapIni.RemoveSection(sectionName);

            var tilesToSave = new List<IsoMapPack5Tile>();

            for (int y = 0; y < map.Tiles.Length; y++)
            {
                for (int x = 0; x < map.Tiles[y].Length; x++)
                {
                    var tile = map.Tiles[y][x];
                    if (tile == null)
                        continue;

                    if (tile.Level == 0 && tile.TileIndex == 0)
                        continue;

                    tilesToSave.Add(tile);
                }
            }

            // Typically, removing the height level 0 clear tiles and then sorting 
            // the tiles first by X then by Level and then by TileIndex gives good compression. 
            // https://modenc.renegadeprojects.com/IsoMapPack5

            tilesToSave = tilesToSave.OrderBy(t => t.X).ThenBy(t => t.Level).ThenBy(t => t.TileIndex).ToList();

            // Now we pretty much have to reverse the process done in MapLoader.ReadIsoMapPack

            var buffer = new List<byte>();
            foreach (IsoMapPack5Tile tile in tilesToSave)
            {
                buffer.AddRange(BitConverter.GetBytes(tile.X));
                buffer.AddRange(BitConverter.GetBytes(tile.Y));
                buffer.AddRange(BitConverter.GetBytes(tile.TileIndex));
                buffer.Add(tile.SubTileIndex);
                buffer.Add(tile.Level);
                buffer.Add(tile.IceGrowth);
            }

            const int maxOutputSize = 8192;
            // generate IsoMapPack5 blocks
            int processedBytes = 0;
            List<byte> finalData = new List<byte>();
            List<byte> block = new List<byte>(maxOutputSize);
            while (buffer.Count > processedBytes)
            {
                ushort blockOutputSize = (ushort)Math.Min(buffer.Count - processedBytes, maxOutputSize);
                for (int i = processedBytes; i < processedBytes + blockOutputSize; i++)
                {
                    block.Add(buffer[i]);
                }

                byte[] compressedBlock = MiniLZO.MiniLZO.Compress(block.ToArray());
                // InputSize
                finalData.AddRange(BitConverter.GetBytes((ushort)compressedBlock.Length));
                // OutputSize
                finalData.AddRange(BitConverter.GetBytes(blockOutputSize));
                // actual data
                finalData.AddRange(compressedBlock);

                processedBytes += blockOutputSize;
                block.Clear();
            }

            // Base64 encode
            string base64String = Convert.ToBase64String(finalData.ToArray());
            const int maxIsoMapPackEntryLineLength = 70;
            int lineIndex = 1; // TS/RA2 IsoMapPack5 is indexed starting from 1
            int processedChars = 0;

            var section = new IniSection(sectionName);
            mapIni.AddSection(section);

            while (processedChars < base64String.Length)
            {
                int length = Math.Min(base64String.Length - processedChars, maxIsoMapPackEntryLineLength);

                string substring = base64String.Substring(processedChars, length);
                section.SetStringValue(lineIndex.ToString(), substring);
                lineIndex++;
                processedChars += length;
            }
        }

        public static void WriteAircraft(IMap map, IniFile mapIni)
        {
            const string sectionName = "Aircraft";

            mapIni.RemoveSection(sectionName);
            if (map.Aircraft.Count == 0)
                return;

            var section = new IniSection(sectionName);
            mapIni.AddSection(section);

            for (int i = 0; i < map.Aircraft.Count; i++)
            {
                var aircraft = map.Aircraft[i];

                // INDEX = OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

                string attachedTag = aircraft.AttachedTag == null ? Constants.NoneValue2 : aircraft.AttachedTag.ID;
                string value = $"{aircraft.Owner.ININame},{aircraft.ObjectType.ININame},{aircraft.HP}," +
                               $"{aircraft.Position.X},{aircraft.Position.Y},{aircraft.Facing}," +
                               $"{aircraft.Mission},{attachedTag},{aircraft.Veterancy}," +
                               $"{aircraft.AutocreateNoRecruitable},{aircraft.AutocreateYesRecruitable}";

                section.SetStringValue(i.ToString(), value);
            }
        }
    }
}
