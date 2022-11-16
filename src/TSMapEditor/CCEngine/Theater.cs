using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using TSMapEditor.Models;

namespace TSMapEditor.CCEngine
{
    public class LATGround
    {
        public LATGround(string displayName, TileSet groundTileSet, TileSet transitionTileSet, TileSet baseTileSet)
        {
            DisplayName = displayName;
            GroundTileSet = groundTileSet;
            TransitionTileSet = transitionTileSet;
            BaseTileSet = baseTileSet;
        }

        public string DisplayName { get; }
        public TileSet GroundTileSet { get; }
        public TileSet TransitionTileSet { get; }
        public TileSet BaseTileSet { get; }
    }

    public class Theater : INIDefineable
    {
        public Theater(string name)
        {
            UIName = name;
        }

        public Theater(string uiName, string configIniName, string contentMixName,
            string paletteName, string unitPaletteName, string fileExtension,
            char newTheaterBuildingLetter)
        {
            UIName = uiName;
            ConfigINIPath = configIniName;
            ContentMIXName = contentMixName;
            TerrainPaletteName = paletteName;
            UnitPaletteName = unitPaletteName;
            FileExtension = fileExtension;
            NewTheaterBuildingLetter = newTheaterBuildingLetter;
        }

        public string UIName { get; }
        public string ConfigINIPath { get; set; }
        public string ContentMIXName { get; set; }
        public string TerrainPaletteName { get; set; }
        public string UnitPaletteName { get; set; }
        public string FileExtension { get; set; }
        public char NewTheaterBuildingLetter { get; set; }

        public void ReadConfigINI(string baseDirectoryPath)
        {
            TileSets.Clear();

            IniFile theaterIni = new IniFile(Path.Combine(baseDirectoryPath, ConfigINIPath));
            int i;

            for (i = 0; i < 10000; i++)
            {
                IniSection tileSetSection = theaterIni.GetSection(string.Format("TileSet{0:D4}", i));

                if (tileSetSection == null)
                    break;

                TileSet tileSet = new TileSet(i);
                tileSet.Read(tileSetSection);
                TileSets.Add(tileSet);
            }

            i = 1;
            while (true)
            {
                if (!InitLATGround(theaterIni, $"Ground{i}Tile", $"Ground{i}Lat", $"Ground{i}Base", $"Ground{i}Name", null))
                    break;

                i++;
            }

            InitLATGround(theaterIni, "PvmntTile", "ClearToPvmntLat", null, null, "Pavement");
        }

        private bool InitLATGround(IniFile theaterIni, string tileSetKey, string transitionTileSetKey, string baseTileSetKey, string nameKey, string defaultName)
        {
            int groundTileSetIndex = theaterIni.GetIntValue("General", tileSetKey, -1);
            int transitionTileSetIndex = theaterIni.GetIntValue("General", transitionTileSetKey, -1);

            int baseTileSetIndex = -1;
            if (!string.IsNullOrEmpty(baseTileSetKey))
                baseTileSetIndex = theaterIni.GetIntValue("General", baseTileSetKey, -1);

            if (groundTileSetIndex < 0 || transitionTileSetIndex < 0)
                return false;

            if (groundTileSetIndex >= TileSets.Count || transitionTileSetIndex >= TileSets.Count)
                return false;

            string displayName = defaultName;
            if (!string.IsNullOrEmpty(nameKey))
                displayName = theaterIni.GetStringValue("General", nameKey, displayName);

            if (displayName == null)
            {
                string groundTileSetName = TileSets[groundTileSetIndex].SetName;
                displayName = groundTileSetName.Substring(0, Math.Min(groundTileSetName.Length, 4));
            }

            LATGrounds.Add(new LATGround(
                displayName,
                TileSets[groundTileSetIndex],
                TileSets[transitionTileSetIndex],
                baseTileSetIndex > -1 ? TileSets[baseTileSetIndex] : TileSets[0]));

            return true;
        }

        public List<TileSet> TileSets = new List<TileSet>();
        public List<LATGround> LATGrounds = new List<LATGround>();
    }
}
