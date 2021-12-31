using Microsoft.Win32;
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
        private const string DirectoryPrefix = "<DIR> ";

        public MainMenu(WindowManager windowManager) : base(windowManager)
        {
        }

        private string gameDirectory;

        private EditorTextBox tbGameDirectory;
        private EditorTextBox tbMapPath;
        private EditorButton btnLoad;
        private EditorListBox lbFileList;
        private int loadingStage;

        string fileListDirectoryPath = string.Empty;

        public override void Initialize()
        {
            Name = nameof(MainMenu);
            Width = 370;

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
            if (string.IsNullOrWhiteSpace(tbGameDirectory.Text))
            {
                ReadGameInstallDirectoryFromRegistry();
            }
            tbGameDirectory.TextChanged += TbGameDirectory_TextChanged;
            AddChild(tbGameDirectory);

            var lblMapPath = new XNALabel(WindowManager);
            lblMapPath.Name = nameof(lblMapPath);
            lblMapPath.X = Constants.UIEmptySideSpace;
            lblMapPath.Y = tbGameDirectory.Bottom + Constants.UIEmptyTopSpace;
            lblMapPath.Text = "Path of the map file to load (can be relative to game directory):";
            AddChild(lblMapPath);

            tbMapPath = new EditorTextBox(WindowManager);
            tbMapPath.Name = nameof(tbMapPath);
            tbMapPath.X = Constants.UIEmptySideSpace;
            tbMapPath.Y = lblMapPath.Bottom + Constants.UIVerticalSpacing;
            tbMapPath.Width = Width - Constants.UIEmptySideSpace * 2;
            tbMapPath.Text = UserSettings.Instance.LastScenarioPath;
            AddChild(tbMapPath);

            var lblDirectoryListing = new XNALabel(WindowManager);
            lblDirectoryListing.Name = nameof(lblDirectoryListing);
            lblDirectoryListing.X = Constants.UIEmptySideSpace;
            lblDirectoryListing.Y = tbMapPath.Bottom + Constants.UIVerticalSpacing * 2;
            lblDirectoryListing.Text = "Alternatively, select a map file below:";
            AddChild(lblDirectoryListing);

            lbFileList = new EditorListBox(WindowManager);
            lbFileList.Name = nameof(lbFileList);
            lbFileList.X = Constants.UIEmptySideSpace;
            lbFileList.Y = lblDirectoryListing.Bottom + Constants.UIVerticalSpacing;
            lbFileList.Width = Width - Constants.UIEmptySideSpace * 2;
            lbFileList.Height = 300;
            lbFileList.SelectedIndexChanged += LbFileList_SelectedIndexChanged;
            lbFileList.DoubleLeftClick += LbFileList_DoubleLeftClick;
            AddChild(lbFileList);

            btnLoad = new EditorButton(WindowManager);
            btnLoad.Name = nameof(btnLoad);
            btnLoad.Width = 100;
            btnLoad.Text = "Load";
            btnLoad.Y = lbFileList.Bottom + Constants.UIEmptyTopSpace;
            AddChild(btnLoad);
            btnLoad.CenterOnParentHorizontally();
            btnLoad.LeftClick += BtnLoad_LeftClick;

            Height = btnLoad.Bottom + Constants.UIEmptyBottomSpace;


            if (Path.IsPathRooted(tbMapPath.Text))
            {
                fileListDirectoryPath = Path.GetDirectoryName(tbMapPath.Text);
            }
            else
            {
                fileListDirectoryPath = Path.GetDirectoryName(tbGameDirectory.Text + tbMapPath.Text);
            }

            fileListDirectoryPath = fileListDirectoryPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            ListFiles();

            base.Initialize();
        }

        private void ReadGameInstallDirectoryFromRegistry()
        {
            try
            {
                RegistryKey key;
                key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\DawnOfTheTiberiumAge");
                object value = key.GetValue("InstallPath", string.Empty);
                if (!(value is string valueAsString))
                {
                    tbGameDirectory.Text = string.Empty;
                }
                else
                {
                    tbGameDirectory.Text = valueAsString;
                }

                key.Close();
            }
            catch (Exception ex)
            {
                tbGameDirectory.Text = string.Empty;
                Logger.Log("Failed to read game installation path from the Windows registry! Exception message: " + ex.Message);
            }
        }

        private void LbFileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbFileList.SelectedItem == null)
                return;

            if (lbFileList.SelectedItem.Tag != null)
                return;

            // Select file
            if (!fileListDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                fileListDirectoryPath += Path.DirectorySeparatorChar;

            tbMapPath.Text = fileListDirectoryPath + lbFileList.SelectedItem.Text;
        }

        private void TbGameDirectory_TextChanged(object sender, EventArgs e)
        {
            fileListDirectoryPath = tbGameDirectory.Text;
            ListFiles();
        }

        private void LbFileList_DoubleLeftClick(object sender, EventArgs e)
        {
            if (lbFileList.SelectedItem == null)
                return;

            if (lbFileList.SelectedIndex == 0)
            {
                // Special case -- go up a directory
                fileListDirectoryPath = Path.GetDirectoryName(fileListDirectoryPath.TrimEnd('/', '\\'));
                ListFiles();
                return;
            }

            if (!fileListDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                fileListDirectoryPath += Path.DirectorySeparatorChar;

            if (lbFileList.SelectedItem.Tag != null)
            {
                // Browse to next directory
                fileListDirectoryPath = fileListDirectoryPath + lbFileList.SelectedItem.Text.Substring(DirectoryPrefix.Length) + Path.DirectorySeparatorChar;
                ListFiles();
                return;
            }

            // Select file
            tbMapPath.Text = fileListDirectoryPath + lbFileList.SelectedItem.Text;
            BtnLoad_LeftClick(this, EventArgs.Empty);
        }

        private void ListFiles()
        {
            lbFileList.SelectedIndex = -1;
            lbFileList.Clear();

            if (string.IsNullOrWhiteSpace(fileListDirectoryPath) || !Directory.Exists(fileListDirectoryPath))
            {
                return;
            }

            lbFileList.AddItem(new XNAListBoxItem(".. <Directory Up>", Color.Gray) { Tag = new object() });

            var directories = Directory.GetDirectories(fileListDirectoryPath);
            foreach (string dir in directories)
            {
                string dirName = dir.Substring(dir.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                lbFileList.AddItem(new XNAListBoxItem(DirectoryPrefix + dirName, Color.LightGray) { Tag = new object() }); // Yay for wasting memory
            }

            var files = Directory.GetFiles(fileListDirectoryPath);
            foreach (string file in files)
            {
                lbFileList.AddItem(Path.GetFileName(file));
            }
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

            string mapPath = Path.Combine(gameDirectory, tbMapPath.Text);
            if (Path.IsPathRooted(tbMapPath.Text))
                mapPath = tbMapPath.Text;

            if (!File.Exists(mapPath))
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
            IniFile fsaIni = new IniFile(Path.Combine(gameDirectory, "INI/FSA.INI"));
            IniFile.ConsolidateIniFiles(artFSIni, fsaIni);
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
