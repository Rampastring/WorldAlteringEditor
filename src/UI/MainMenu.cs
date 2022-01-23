using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.Settings;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.Windows;
using TSMapEditor.UI.Windows.MainMenuWindows;

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
        private FileBrowserListBox lbFileList;

        private SettingsPanel settingsPanel;

        private int loadingStage;

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

            lbFileList = new FileBrowserListBox(WindowManager);
            lbFileList.Name = nameof(lbFileList);
            lbFileList.X = Constants.UIEmptySideSpace;
            lbFileList.Y = lblDirectoryListing.Bottom + Constants.UIVerticalSpacing;
            lbFileList.Width = Width - Constants.UIEmptySideSpace * 2;
            lbFileList.Height = 300;
            lbFileList.FileSelected += LbFileList_FileSelected;
            lbFileList.FileDoubleLeftClick += LbFileList_FileDoubleLeftClick;
            AddChild(lbFileList);

            btnLoad = new EditorButton(WindowManager);
            btnLoad.Name = nameof(btnLoad);
            btnLoad.Width = 100;
            btnLoad.Text = "Load";
            btnLoad.Y = lbFileList.Bottom + Constants.UIEmptyTopSpace;
            btnLoad.X = lbFileList.Right - btnLoad.Width;
            AddChild(btnLoad);
            btnLoad.LeftClick += BtnLoad_LeftClick;

            var btnCreateNewMap = new EditorButton(WindowManager);
            btnCreateNewMap.Name = nameof(btnCreateNewMap);
            btnCreateNewMap.Width = 100;
            btnCreateNewMap.Text = "New Map...";
            btnCreateNewMap.X = lbFileList.X;
            btnCreateNewMap.Y = btnLoad.Y;
            AddChild(btnCreateNewMap);
            btnCreateNewMap.LeftClick += BtnCreateNewMap_LeftClick;

            Height = btnLoad.Bottom + Constants.UIEmptyBottomSpace;

            settingsPanel = new SettingsPanel(WindowManager);
            settingsPanel.Name = nameof(settingsPanel);
            settingsPanel.X = Width;
            settingsPanel.Y = Constants.UIEmptyTopSpace;
            settingsPanel.Height = Height - Constants.UIEmptyTopSpace - Constants.UIEmptyBottomSpace;
            AddChild(settingsPanel);
            Width += settingsPanel.Width + Constants.UIEmptySideSpace;

            string directoryPath;

            if (Path.IsPathRooted(tbMapPath.Text))
            {
                directoryPath = Path.GetDirectoryName(tbMapPath.Text);
            }
            else
            {
                directoryPath = Path.GetDirectoryName(tbGameDirectory.Text + tbMapPath.Text);
            }

            directoryPath = directoryPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            lbFileList.DirectoryPath = directoryPath;

            base.Initialize();
        }

        private void LbFileList_FileSelected(object sender, FileSelectionEventArgs e)
        {
            tbMapPath.Text = e.FilePath;
        }

        private void BtnCreateNewMap_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

            ApplySettings();
            WindowManager.RemoveControl(this);
            WindowManager.AddAndInitializeControl(new CreateNewMapWindow(WindowManager, gameDirectory));
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

        private void TbGameDirectory_TextChanged(object sender, EventArgs e)
        {
            lbFileList.DirectoryPath = tbGameDirectory.Text;
        }

        private void LbFileList_FileDoubleLeftClick(object sender, EventArgs e)
        {
            BtnLoad_LeftClick(this, EventArgs.Empty);
        }

        private bool CheckGameDirectory()
        {
            if (!File.Exists(Path.Combine(tbGameDirectory.Text, "DTA.exe")))
            {
                EditorMessageBox.Show(WindowManager,
                    "Invalid game directory",
                    "DTA.exe not found, please check that you typed the correct game directory.",
                    MessageBoxButtons.OK);

                return false;
            }

            gameDirectory = tbGameDirectory.Text;
            if (!gameDirectory.EndsWith("/") && !gameDirectory.EndsWith("\\"))
                gameDirectory += "/";

            return true;
        }

        private void ApplySettings()
        {
            settingsPanel.ApplySettings();

            UserSettings.Instance.LastScenarioPath.UserDefinedValue = tbMapPath.Text;

            bool fullscreenWindowed = UserSettings.Instance.FullscreenWindowed.GetValue();
            bool borderless = UserSettings.Instance.Borderless.GetValue();
            if (fullscreenWindowed && !borderless)
                throw new InvalidOperationException("Borderless= cannot be set to false if FullscreenWindowed= is enabled.");

            WindowManager.InitGraphicsMode(
                UserSettings.Instance.ResolutionWidth.GetValue(),
                UserSettings.Instance.ResolutionHeight.GetValue(),
                fullscreenWindowed);

            WindowManager.SetRenderResolution(UserSettings.Instance.RenderResolutionWidth.GetValue(), UserSettings.Instance.RenderResolutionHeight.GetValue());
            WindowManager.CenterOnScreen();
            WindowManager.SetBorderlessMode(borderless);

            WindowManager.CenterControlOnScreen(this);

            UserSettings.Instance.SaveSettings();
        }

        private void BtnLoad_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

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

            ApplySettings();
        }

        public override void Update(GameTime gameTime)
        {
            if (loadingStage > 2)
                LoadExisting(tbMapPath.Text);

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

        private void LoadExisting(string mapPath)
        {
            MapSetup.InitializeMap(WindowManager, gameDirectory, false, mapPath, null, Point2D.Zero);
            WindowManager.RemoveControl(this);
        }
    }
}
