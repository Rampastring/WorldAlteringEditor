using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.Settings;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.Windows;

namespace TSMapEditor.UI
{
    public class MainMenu : EditorPanel
    {
        public MainMenu(WindowManager windowManager) : base(windowManager)
        {
        }

        private string gameDirectory;

        private EditorTextBox tbGameDirectory;
        private EditorTextBox tbMapPath;
        private EditorButton btnLoad;
        private int loadingStage;

        public override void Initialize()
        {
            Name = nameof(MainMenu);
            Width = 350;

            var lblGameDirectory = new XNALabel(WindowManager);
            lblGameDirectory.Name = nameof(lblGameDirectory);
            lblGameDirectory.X = Constants.UIEmptySideSpace;
            lblGameDirectory.Y = Constants.UIEmptyTopSpace;
            lblGameDirectory.Text = "Path to the game directory:";
            AddChild(lblGameDirectory);

            tbGameDirectory = new EditorTextBox(WindowManager);
            tbGameDirectory.Name = nameof(tbGameDirectory);
            tbGameDirectory.X = Constants.UIEmptySideSpace;
            tbGameDirectory.Y = lblGameDirectory.Bottom + Constants.UIVerticalSpacing;
            tbGameDirectory.Width = Width - Constants.UIEmptySideSpace * 2;
            tbGameDirectory.Text = UserSettings.Instance.GameDirectory;
            AddChild(tbGameDirectory);

            var lblDescription = new XNALabel(WindowManager);
            lblDescription.Name = nameof(lblDescription);
            lblDescription.X = Constants.UIEmptySideSpace;
            lblDescription.Y = tbGameDirectory.Bottom + Constants.UIEmptyTopSpace;
            lblDescription.Text = "Path of the map file to load (relative to game directory):";
            AddChild(lblDescription);

            tbMapPath = new EditorTextBox(WindowManager);
            tbMapPath.Name = nameof(tbMapPath);
            tbMapPath.X = Constants.UIEmptySideSpace;
            tbMapPath.Y = lblDescription.Bottom + Constants.UIVerticalSpacing;
            tbMapPath.Width = Width - Constants.UIEmptySideSpace * 2;
            tbMapPath.Text = UserSettings.Instance.LastScenarioPath;
            AddChild(tbMapPath);

            btnLoad = new EditorButton(WindowManager);
            btnLoad.Name = nameof(btnLoad);
            btnLoad.Width = 100;
            btnLoad.Text = "Load";
            btnLoad.Y = tbMapPath.Bottom + Constants.UIEmptyTopSpace;
            AddChild(btnLoad);
            btnLoad.CenterOnParentHorizontally();
            btnLoad.LeftClick += BtnLoad_LeftClick;

            Height = btnLoad.Bottom + Constants.UIEmptyBottomSpace;

            base.Initialize();
        }

        private void BtnLoad_LeftClick(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(tbGameDirectory.Text, "DTA.exe")))
            {
                EditorMessageBox.Show(WindowManager,
                    "Invalid game directory",
                    "DTA.exe not found, please check that you typed the correct game directory.",
                    MessageBoxButtons.OK);

                return;
            }

            gameDirectory = tbGameDirectory.Text;
            if (!gameDirectory.EndsWith("/") && !gameDirectory.EndsWith("\\"))
                gameDirectory += "/";

            UserSettings.Instance.GameDirectory.UserDefinedValue = gameDirectory;

            if (!File.Exists(Path.Combine(gameDirectory, tbMapPath.Text)))
            {
                EditorMessageBox.Show(WindowManager,
                    "Invalid map path",
                    "Specified map file not found. Please re-check the path to the map file.",
                    MessageBoxButtons.OK);

                return;
            }

            btnLoad.Text = "Loading";
            loadingStage = 1;
            
            UserSettings.Instance.LastScenarioPath.UserDefinedValue = tbMapPath.Text;
            UserSettings.Instance.SaveSettings();
        }

        public override void Update(GameTime gameTime)
        {
            if (loadingStage > 2)
                InitTest(tbMapPath.Text);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (loadingStage > 0)
            {
                loadingStage++;
            }
        }

        private void InitTest(string mapPath)
        {
            IniFile rulesIni = new IniFile(Path.Combine(gameDirectory, "INI/Rules.ini"));
            IniFile firestormIni = new IniFile(Path.Combine(gameDirectory, "INI/Enhance.ini"));
            IniFile artIni = new IniFile(Path.Combine(gameDirectory, "INI/Art.ini"));
            IniFile artFSIni = new IniFile(Path.Combine(gameDirectory, "INI/ArtE.INI"));
            IniFile mapIni = new IniFile(Path.Combine(gameDirectory, mapPath));
            //IniFile mapIni = new IniFile(Path.Combine(GameDirectory, "Maps/Default/a_buoyant_city.map"));
            Map map = new Map();
            map.LoadExisting(rulesIni, firestormIni, artIni, artFSIni, mapIni);

            Console.WriteLine();
            Console.WriteLine("Map loaded.");

            var theater = map.EditorConfig.Theaters.Find(t => t.UIName.Equals(map.TheaterName, StringComparison.InvariantCultureIgnoreCase));
            if (theater == null)
            {
                throw new InvalidOperationException("Theater of map not found: " + map.TheaterName);
            }
            theater.ReadConfigINI(gameDirectory);

            CCFileManager ccFileManager = new CCFileManager();
            ccFileManager.GameDirectory = gameDirectory;
            ccFileManager.ReadConfig();
            ccFileManager.LoadPrimaryMixFile(theater.ContentMIXName);

            TheaterGraphics theaterGraphics = new TheaterGraphics(GraphicsDevice, theater, ccFileManager, map.Rules);
            map.TheaterInstance = theaterGraphics;

            var uiManager = new UIManager(WindowManager, map, theaterGraphics);
            WindowManager.AddAndInitializeControl(uiManager);

            WindowManager.RemoveControl(this);
        }
    }
}
