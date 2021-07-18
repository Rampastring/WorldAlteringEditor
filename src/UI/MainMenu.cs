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

namespace TSMapEditor.UI
{
    public class MainMenu : EditorPanel
    {
        public MainMenu(WindowManager windowManager, string gameDirectory) : base(windowManager)
        {
            GameDirectory = gameDirectory;
        }

        private readonly string GameDirectory;

        private XNATextBox tbMapPath;
        private EditorButton btnLoad;
        private int loadingStage;

        public override void Initialize()
        {
            Name = nameof(MainMenu);
            Width = 350;

            var lblDescription = new XNALabel(WindowManager);
            lblDescription.Name = nameof(lblDescription);
            lblDescription.X = Constants.UIEmptySideSpace;
            lblDescription.Y = Constants.UIEmptyTopSpace;
            lblDescription.Text = "Path of the map file to load (relative to game directory):";
            AddChild(lblDescription);

            tbMapPath = new XNATextBox(WindowManager);
            tbMapPath.Name = nameof(tbMapPath);
            tbMapPath.X = Constants.UIEmptySideSpace;
            tbMapPath.Y = lblDescription.Bottom + Constants.UIVerticalSpacing;
            tbMapPath.Width = Width - Constants.UIEmptySideSpace * 2;
            tbMapPath.Height = Constants.UITextBoxHeight;
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
            if (!File.Exists(Path.Combine(GameDirectory, tbMapPath.Text)))
            {
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
            IniFile rulesIni = new IniFile(Path.Combine(GameDirectory, "INI/Rules.ini"));
            IniFile firestormIni = new IniFile(Path.Combine(GameDirectory, "INI/Enhance.ini"));
            IniFile artIni = new IniFile(Path.Combine(GameDirectory, "INI/Art.ini"));
            IniFile artFSIni = new IniFile(Path.Combine(GameDirectory, "INI/ArtE.INI"));
            IniFile mapIni = new IniFile(Path.Combine(GameDirectory, mapPath));
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
            theater.ReadConfigINI(GameDirectory);

            CCFileManager ccFileManager = new CCFileManager();
            ccFileManager.GameDirectory = GameDirectory;
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
