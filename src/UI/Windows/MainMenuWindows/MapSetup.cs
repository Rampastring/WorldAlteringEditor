using Rampastring.Tools;
using Rampastring.XNAUI;
using System;
using System.IO;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Initialization;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Windows.MainMenuWindows
{
    /// <summary>
    /// Helper class for setting up a map.
    /// </summary>
    public static class MapSetup
    {
        /// <summary>
        /// Tries to load a map. If successful, loads graphics for the theater and
        /// sets up the editor UI, and returns null once done. If loading the map
        /// fails, returns an error message.
        /// </summary>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="gameDirectory">The path to the game directory.</param>
        /// <param name="createNew">Whether a new map should be created (instead of loading an existing map).</param>
        /// <param name="existingMapPath">The path to the existing map file to load, if loading an existing map. Can be null if creating a new map.</param>
        /// <param name="newMapTheater">The theater of the map, if creating a new map.</param>
        /// <param name="newMapSize">The size of the map, if creating a new map.</param>
        /// <returns>Null of loading the map was successful, otherwise an error message.</returns>
        public static string InitializeMap(WindowManager windowManager, string gameDirectory, bool createNew, string existingMapPath, string newMapTheater, Point2D newMapSize)
        {
            IniFile rulesIni = new IniFile(Path.Combine(gameDirectory, "INI/Rules.ini"));
            IniFile firestormIni = new IniFile(Path.Combine(gameDirectory, "INI/Enhance.ini"));
            IniFile artIni = new IniFile(Path.Combine(gameDirectory, "INI/Art.ini"));
            IniFile artFSIni = new IniFile(Path.Combine(gameDirectory, "INI/ArtE.INI"));
            IniFile artOverridesIni = new IniFile(Path.Combine(Environment.CurrentDirectory, "Config/ArtOverrides.ini"));
            IniFile.ConsolidateIniFiles(artFSIni, artOverridesIni);

            var tutorialLines = new TutorialLines(Path.Combine(gameDirectory, "INI/Tutorial.ini"));

            Map map = new Map();

            if (createNew)
            {
                map.InitNew(rulesIni, firestormIni, artIni, artFSIni, newMapTheater, newMapSize);
            }
            else
            {
                try
                {
                    IniFile mapIni = new IniFile(Path.Combine(gameDirectory, existingMapPath));

                    MapLoader.PreCheckMapIni(mapIni);

                    map.LoadExisting(rulesIni, firestormIni, artIni, artFSIni, mapIni);
                }
                catch (IniParseException ex)
                {
                    return "The selected file does not appear to be a proper map file (INI file). Maybe it's corrupted?\r\n\r\nReturned error: " + ex.Message;
                }
                catch (MapLoadException ex)
                {
                    return "Failed to load the selected map file.\r\n\r\nReturned error: " + ex.Message;
                }
            }

            map.Rules.TutorialLines = tutorialLines;

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

            return null;
        }
    }
}
