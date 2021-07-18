using Rampastring.Tools;
using System.Collections.Generic;
using TSMapEditor.Models;

namespace TSMapEditor.CCEngine
{
    public class LATGround
    {
        public LATGround(TileSet groundTileSet, TileSet transitionTileSet, TileSet baseTileSet)
        {
            GroundTileSet = groundTileSet;
            TransitionTileSet = transitionTileSet;
            BaseTileSet = baseTileSet;
        }

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

            IniFile theaterIni = new IniFile(baseDirectoryPath + ConfigINIPath);
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
                int groundTileSetIndex = theaterIni.GetIntValue("General", $"Ground{i}Tile", -1);
                int transitionTileSetIndex = theaterIni.GetIntValue("General", $"Ground{i}Lat", -1);
                int baseTileSetIndex = theaterIni.GetIntValue("General", $"Ground{i}Base", -1);
                if (groundTileSetIndex < 0 || transitionTileSetIndex < 0)
                    break;

                if (groundTileSetIndex >= TileSets.Count || transitionTileSetIndex >= TileSets.Count)
                    break;

                LATGrounds.Add(new LATGround(
                    TileSets[groundTileSetIndex],
                    TileSets[transitionTileSetIndex],
                    baseTileSetIndex > -1 ? TileSets[baseTileSetIndex] : null));

                i++;
            }
        }

        public List<TileSet> TileSets = new List<TileSet>();
        public List<LATGround> LATGrounds = new List<LATGround>();
    }
}
