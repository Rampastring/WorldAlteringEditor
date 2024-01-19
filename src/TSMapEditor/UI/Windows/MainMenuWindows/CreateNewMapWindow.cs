using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.GameMath;
using TSMapEditor.Misc;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows.MainMenuWindows
{
    public class CreateNewMapEventArgs : EventArgs
    {
        public CreateNewMapEventArgs(string theater, Point2D mapSize, Texture2D terrainImage = null, Texture2D heightmap = null, byte basicLevel = 0, string importConfig = null)
        {
            Theater = theater;
            MapSize = mapSize;
            BasicLevel = basicLevel;
            TerrainImage = terrainImage;
            Heightmap = heightmap;
            ImportConfig = importConfig;
        }

        public string Theater { get; }
        public Point2D MapSize { get; }

        public byte BasicLevel { get; }

        public Texture2D TerrainImage { get; }
        public Texture2D Heightmap { get; }
        public string ImportConfig { get; }
    }

    public class CreateNewMapWindow : INItializableWindow
    {
        private const int MinMapSize = 50;
        private const int MaxMapSize = 512;

        public CreateNewMapWindow(WindowManager windowManager, bool canExit) : base(windowManager)
        {
            this.canExit = canExit;
        }

        public event EventHandler<CreateNewMapEventArgs> OnCreateNewMap;

        private readonly bool canExit;

        private XNADropDown ddTheater;
        private EditorNumberTextBox tbWidth;
        private EditorNumberTextBox tbHeight;
        private EditorNumberTextBox tbBasicHeight;
        private EditorButton btnSelectTerrainImage;
        private EditorTextBox tbTerrainImage;
        private EditorButton btnSelectHeightmap;
        private EditorTextBox tbHeightmap;
        private EditorButton btnSelectImportConfig;
        private EditorTextBox tbImportConfig;
        private EditorButton btnApplyTerrainImageRatio;

        public override void Initialize()
        {
            HasCloseButton = canExit;

            Name = nameof(CreateNewMapWindow);
            base.Initialize();

            ddTheater = FindChild<XNADropDown>(nameof(ddTheater));
            tbWidth = FindChild<EditorNumberTextBox>(nameof(tbWidth));
            tbHeight = FindChild<EditorNumberTextBox>(nameof(tbHeight));
            tbBasicHeight = FindChild<EditorNumberTextBox>(nameof(tbBasicHeight));

            btnSelectTerrainImage = FindChild<EditorButton>(nameof(btnSelectTerrainImage));
            tbTerrainImage = FindChild<EditorTextBox>(nameof(tbTerrainImage));
            
            btnSelectHeightmap = FindChild<EditorButton>(nameof(btnSelectHeightmap));
            tbHeightmap = FindChild<EditorTextBox>(nameof(tbHeightmap));

            btnSelectImportConfig = FindChild<EditorButton>(nameof(btnSelectImportConfig));
            tbImportConfig = FindChild<EditorTextBox>(nameof(tbImportConfig));

            btnApplyTerrainImageRatio = FindChild<EditorButton>(nameof(btnApplyTerrainImageRatio));

            FindChild<EditorButton>("btnCreate").LeftClick += BtnCreate_LeftClick;
            btnSelectTerrainImage.LeftClick += BtnBrowseTerrainImage_LeftClick;
            btnSelectHeightmap.LeftClick += BtnBrowseHeightmap_LeftClick;
            btnSelectTerrainImage.LeftClick += BtnBrowseImportConfig_LeftClick;
            btnApplyTerrainImageRatio.LeftClick += BtnApplyTerrainImageRatio_LeftClick;

            ddTheater.SelectedIndex = 0;

            CenterOnParent();
        }

        public void Open()
        {
            Show();
        }

        private void BtnCreate_LeftClick(object sender, EventArgs e)
        {
            if (tbWidth.Value < MinMapSize)
            {
                EditorMessageBox.Show(WindowManager, "Map too narrow", "Map width must be at least " + MinMapSize + " cells.", MessageBoxButtons.OK);
                return;
            }

            if (tbHeight.Value < MinMapSize)
            {
                EditorMessageBox.Show(WindowManager, "Map too small", "Map height must be at least " + MinMapSize + " cells.", MessageBoxButtons.OK);
                return;
            }

            if (tbWidth.Value > Constants.MaxMapWidth)
            {
                EditorMessageBox.Show(WindowManager, "Map too wide", "Map width cannot exceed " + Constants.MaxMapWidth + " cells.", MessageBoxButtons.OK);
                return;
            }

            if (tbHeight.Value > Constants.MaxMapHeight)
            {
                EditorMessageBox.Show(WindowManager, "Map too long", "Map height cannot exceed " + Constants.MaxMapHeight + " cells.", MessageBoxButtons.OK);
                return;
            }

            if (tbWidth.Value + tbHeight.Value > MaxMapSize)
            {
                EditorMessageBox.Show(WindowManager, "Map too large", "Map width + height cannot exceed " + MaxMapSize + " cells.", MessageBoxButtons.OK);
                return;
            }

            var bmpTerrainImage = LoadTerrainImage();
            var bmpHeightmap = LoadHeightmap();

            OnCreateNewMap?.Invoke(this, new CreateNewMapEventArgs(
                ddTheater.SelectedItem.Text, 
                new Point2D(tbWidth.Value, tbHeight.Value),
                bmpTerrainImage, bmpHeightmap,
                (byte) tbBasicHeight.Value,
                tbImportConfig.Text
            ));
            WindowManager.RemoveControl(this);
        }

        private Texture2D LoadTerrainImage()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbTerrainImage.Text))
                    return null;

                Texture2D texture;
                using (FileStream fileStream = new FileStream(tbTerrainImage.Text, FileMode.Open))
                {
                    texture = Texture2D.FromStream(GraphicsDevice, fileStream);
                }
                return texture;
            }
            catch (Exception ex)
            {
                EditorMessageBox.Show(WindowManager, "Import error", "File could not be open.", MessageBoxButtons.OK);
                return null;
            }
        }

        private Texture2D LoadHeightmap()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbHeightmap.Text))
                    return null;

                Texture2D texture;
                using (FileStream fileStream = new FileStream(tbHeightmap.Text, FileMode.Open))
                {
                    texture = Texture2D.FromStream(GraphicsDevice, fileStream);
                }
                return texture;
            }
            catch (Exception ex)
            {
                EditorMessageBox.Show(WindowManager, "Import error", "File could not be open.", MessageBoxButtons.OK);
                return null;
            }
        }

        private void BtnBrowseTerrainImage_LeftClick(object sender, EventArgs e)
        {
            FileSystemExtensions.OpenFile(
                onOpened: (fileName) => 
                {
                    tbTerrainImage.Text = fileName;
                },
                filter: Constants.OpenImageFileDialogFilter
            );
        }
        private void BtnBrowseHeightmap_LeftClick(object sender, EventArgs e)
        {
            FileSystemExtensions.OpenFile(
                onOpened: (fileName) => 
                {					
                    tbHeightmap.Text = fileName;
                },
                filter: Constants.OpenImageFileDialogFilter
            );
        }
        private void BtnBrowseImportConfig_LeftClick(object sender, EventArgs e)
        {
            FileSystemExtensions.OpenFile(
                onOpened: (fileName) =>
                {
                    tbImportConfig.Text = fileName;
                },
                filter: Constants.OpenIniFileDialogFilter
            );
        }

        private void BtnApplyTerrainImageRatio_LeftClick(object sender, EventArgs e)
        {
            var img = LoadTerrainImage();
            if(img != null)
            {
                // w1 / h1 = w2 / h2
                // h1 / w1 = h2 / w2
                // w1 = w2 * (h1 / h2)
                // h1 = h2 * (w1 / w2)

                tbHeight.Value = ((int)((float) img.Height * Constants.CellWHRatio * (tbWidth.Value) / (float) img.Width));
                //tbWidth.Value = ((int) ((float) bmpTerrainImage.Width * (float.Parse(tbHeight.Text) / (float) bmpTerrainImage.Height)));
            }
        }
    }
}
