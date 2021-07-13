using Rampastring.Tools;
using System.Collections.Generic;
using TSMapEditor.Models;

namespace TSMapEditor.CCEngine
{
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
            for (int i = 0; i < 10000; i++)
            {
                IniSection tileSetSection = theaterIni.GetSection(string.Format("TileSet{0:D4}", i));

                if (tileSetSection == null)
                    return;

                TileSet tileSet = new TileSet();
                tileSet.Read(tileSetSection);
                TileSets.Add(tileSet);
            }
        }

        public List<TileSet> TileSets = new List<TileSet>();
    }
}
