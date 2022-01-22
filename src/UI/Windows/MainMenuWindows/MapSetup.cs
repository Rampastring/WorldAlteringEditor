using Rampastring.Tools;
using Rampastring.XNAUI;
using System;
using System.IO;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Windows.MainMenuWindows
{
    /// <summary>
    /// Helper class for setting up a map.
    /// </summary>
    public static class MapSetup
    {
        public static void InitializeMap(WindowManager windowManager, string gameDirectory, bool createNew, string existingMapPath, string newMapTheater, Point2D newMapSize)
        {
            IniFile rulesIni = new IniFile(Path.Combine(gameDirectory, "INI/Rules.ini"));
            IniFile firestormIni = new IniFile(Path.Combine(gameDirectory, "INI/Enhance.ini"));
            IniFile artIni = new IniFile(Path.Combine(gameDirectory, "INI/Art.ini"));
            IniFile artFSIni = new IniFile(Path.Combine(gameDirectory, "INI/ArtE.INI"));
            IniFile artOverridesIni = new IniFile(Path.Combine(Environment.CurrentDirectory, "Config/ArtOverrides.ini"));
            IniFile.ConsolidateIniFiles(artFSIni, artOverridesIni);

            Map map = new Map();

            if (createNew)
            {
                map.InitNew(rulesIni, firestormIni, artIni, artFSIni, newMapTheater, newMapSize);
            }
            else
            {
                IniFile mapIni = new IniFile(Path.Combine(gameDirectory, existingMapPath));
                map.LoadExisting(rulesIni, firestormIni, artIni, artFSIni, mapIni);
            }

            Console.WriteLine();
            Console.WriteLine("Map created.");

            Theater theater = map.EditorConfig.Theaters.Find(t => t.UIName.Equals(map.TheaterName, StringComparison.InvariantCultureIgnoreCase));
            if (theater == null)
            {
                throw new InvalidOperationException("Theater of map not found: " + map.TheaterName);
            }
            theater.ReadConfigINI(gameDirectory);

            CCFileManager ccFileManager = new CCFileManager();
            ccFileManager.GameDirectory = gameDirectory;
            ccFileManager.ReadConfig();
            ccFileManager.LoadPrimaryMixFile(theater.ContentMIXName);

            TheaterGraphics theaterGraphics = new TheaterGraphics(windowManager.GraphicsDevice, theater, ccFileManager, map.Rules);
            map.TheaterInstance = theaterGraphics;

            var uiManager = new UIManager(windowManager, map, theaterGraphics);
            windowManager.AddAndInitializeControl(uiManager);
        }
    }
}
