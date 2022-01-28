using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to configure the terrain generator (aka <see cref="ForestGenerator"/>).
    /// </summary>
    public class TerrainGeneratorConfigWindow : EditorWindow
    {
        public TerrainGeneratorConfigWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public TerrainGeneratorConfiguration TerrainGeneratorConfiguration { get; private set; }

        private EditorTextBox[] terrainTypeTextBoxes;
        private EditorNumberTextBox[] terrainTypeOpenChances;
        private EditorNumberTextBox[] terrainTypeOccupiedChances;

        private EditorPopUpSelector[] tileSetSelectors;
        private EditorTextBox[] tileIndices;
        private EditorNumberTextBox[] tileGroupOpenChances;
        private EditorNumberTextBox[] tileGroupOccupiedChances;

        private SelectTileSetWindow selectTileSetWindow;

        private EditorPopUpSelector openedTileSetSelector;

        public override void Initialize()
        {
            Width = 800;
            Name = nameof(TerrainGeneratorConfigWindow);

            const int MaxTerrainTypeGroupCount = 4;
            const int MaxTileGroupCount = 6;

            terrainTypeTextBoxes = new EditorTextBox[MaxTerrainTypeGroupCount];
            terrainTypeOpenChances = new EditorNumberTextBox[MaxTerrainTypeGroupCount];
            terrainTypeOccupiedChances = new EditorNumberTextBox[MaxTerrainTypeGroupCount];

            tileSetSelectors = new EditorPopUpSelector[MaxTileGroupCount];
            tileIndices = new EditorTextBox[MaxTileGroupCount];
            tileGroupOpenChances = new EditorNumberTextBox[MaxTileGroupCount];
            tileGroupOccupiedChances = new EditorNumberTextBox[MaxTileGroupCount];

            selectTileSetWindow = new SelectTileSetWindow(WindowManager, map);
            var tileSetDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectTileSetWindow);
            tileSetDarkeningPanel.Hidden += TileSetDarkeningPanel_Hidden;

            int y = 0;

            var lblHeader = new XNALabel(WindowManager);
            lblHeader.Name = nameof(lblHeader);
            lblHeader.FontIndex = Constants.UIBoldFont;
            lblHeader.Text = "TERRAIN GENERATOR CONFIGURATION";
            lblHeader.Y = Constants.UIEmptyTopSpace;
            AddChild(lblHeader);
            lblHeader.CenterOnParentHorizontally();

            y = lblHeader.Bottom + Constants.UIEmptyTopSpace;

            for (int i = 0; i < MaxTerrainTypeGroupCount; i++)
            {
                var lblTerrainTypes = new XNALabel(WindowManager);
                lblTerrainTypes.Name = nameof(lblTerrainTypes) + i;
                lblTerrainTypes.X = Constants.UIEmptySideSpace;
                lblTerrainTypes.Y = y;
                lblTerrainTypes.FontIndex = Constants.UIBoldFont;
                lblTerrainTypes.Text = $"Terrain Types (Group #{i + 1})";
                AddChild(lblTerrainTypes);

                var tbTerrainTypes = new EditorTextBox(WindowManager);
                tbTerrainTypes.Name = nameof(tbTerrainTypes) + i;
                tbTerrainTypes.X = lblTerrainTypes.X;
                tbTerrainTypes.Y = lblTerrainTypes.Bottom + Constants.UIVerticalSpacing;
                tbTerrainTypes.Width = (Width - 252) - tbTerrainTypes.X - Constants.UIEmptySideSpace;
                AddChild(tbTerrainTypes);
                terrainTypeTextBoxes[i] = tbTerrainTypes;

                var lblOpenChance = new XNALabel(WindowManager);
                lblOpenChance.Name = nameof(lblOpenChance) + i;
                lblOpenChance.X = tbTerrainTypes.Right + Constants.UIHorizontalSpacing;
                lblOpenChance.Y = lblTerrainTypes.Y;
                lblOpenChance.Text = "Open cell chance:";
                AddChild(lblOpenChance);

                var tbOpenChance = new EditorNumberTextBox(WindowManager);
                tbOpenChance.Name = nameof(tbOpenChance) + i;
                tbOpenChance.X = lblOpenChance.X;
                tbOpenChance.Y = tbTerrainTypes.Y;
                tbOpenChance.AllowDecimals = true;
                tbOpenChance.Width = 120;
                AddChild(tbOpenChance);
                terrainTypeOpenChances[i] = tbOpenChance;

                var lblOccupiedChance = new XNALabel(WindowManager);
                lblOccupiedChance.Name = nameof(lblOccupiedChance) + i;
                lblOccupiedChance.X = tbOpenChance.Right + Constants.UIHorizontalSpacing;
                lblOccupiedChance.Y = lblOpenChance.Y;
                lblOccupiedChance.Text = "Occupied cell chance:";
                AddChild(lblOccupiedChance);

                var tbOccupiedChance = new EditorNumberTextBox(WindowManager);
                tbOccupiedChance.Name = nameof(tbOpenChance) + i;
                tbOccupiedChance.X = lblOccupiedChance.X;
                tbOccupiedChance.Y = tbTerrainTypes.Y;
                tbOccupiedChance.AllowDecimals = true;
                tbOccupiedChance.Width = 120;
                AddChild(tbOccupiedChance);
                terrainTypeOccupiedChances[i] = tbOccupiedChance;

                y = tbOccupiedChance.Bottom + Constants.UIEmptyTopSpace;
            }

            y += Constants.UIEmptyTopSpace;

            for (int i = 0; i < MaxTileGroupCount; i++)
            {
                var lblTileSet = new XNALabel(WindowManager);
                lblTileSet.Name = nameof(lblTileSet) + i;
                lblTileSet.X = Constants.UIEmptySideSpace;
                lblTileSet.Y = y;
                lblTileSet.FontIndex = Constants.UIBoldFont;
                lblTileSet.Text = $"Tile Set (Tile Group #{i + 1})";
                AddChild(lblTileSet);

                var selTileSet = new EditorPopUpSelector(WindowManager);
                selTileSet.Name = nameof(selTileSet) + i;
                selTileSet.X = lblTileSet.X;
                selTileSet.Y = lblTileSet.Bottom + Constants.UIVerticalSpacing;
                selTileSet.Width = 200;
                AddChild(selTileSet);
                tileSetSelectors[i] = selTileSet;
                selTileSet.LeftClick += SelTileSet_LeftClick;

                var lblTileIndices = new XNALabel(WindowManager);
                lblTileIndices.Name = nameof(lblTileIndices) + i;
                lblTileIndices.X = selTileSet.Right + Constants.UIHorizontalSpacing;
                lblTileIndices.Y = lblTileSet.Y;
                lblTileIndices.Text = $"Indexes of tiles to place (leave blank for all)";
                AddChild(lblTileIndices);

                var tbTileIndices = new EditorTextBox(WindowManager);
                tbTileIndices.Name = nameof(selTileSet) + i;
                tbTileIndices.X = lblTileIndices.X;
                tbTileIndices.Y = lblTileIndices.Bottom + Constants.UIVerticalSpacing;
                tbTileIndices.Width = 322;
                AddChild(tbTileIndices);
                tileIndices[i] = tbTileIndices;

                var lblOpenChance = new XNALabel(WindowManager);
                lblOpenChance.Name = nameof(lblOpenChance) + i;
                lblOpenChance.X = tbTileIndices.Right + Constants.UIHorizontalSpacing;
                lblOpenChance.Y = lblTileSet.Y;
                lblOpenChance.Text = "Open cell chance:";
                AddChild(lblOpenChance);

                var tbOpenChance = new EditorNumberTextBox(WindowManager);
                tbOpenChance.Name = nameof(tbOpenChance) + i;
                tbOpenChance.X = lblOpenChance.X;
                tbOpenChance.Y = selTileSet.Y;
                tbOpenChance.AllowDecimals = true;
                tbOpenChance.Width = 120;
                AddChild(tbOpenChance);
                tileGroupOpenChances[i] = tbOpenChance;

                var lblOccupiedChance = new XNALabel(WindowManager);
                lblOccupiedChance.Name = nameof(lblOccupiedChance) + i;
                lblOccupiedChance.X = tbOpenChance.Right + Constants.UIHorizontalSpacing;
                lblOccupiedChance.Y = lblOpenChance.Y;
                lblOccupiedChance.Text = "Occupied cell chance:";
                AddChild(lblOccupiedChance);

                var tbOccupiedChance = new EditorNumberTextBox(WindowManager);
                tbOccupiedChance.Name = nameof(tbOpenChance) + i;
                tbOccupiedChance.X = lblOccupiedChance.X;
                tbOccupiedChance.Y = selTileSet.Y;
                tbOccupiedChance.AllowDecimals = true;
                tbOccupiedChance.Width = 120;
                AddChild(tbOccupiedChance);
                tileGroupOccupiedChances[i] = tbOccupiedChance;

                y = tbOccupiedChance.Bottom + Constants.UIEmptyTopSpace;
            }

            var btnApply = new EditorButton(WindowManager);
            btnApply.Name = nameof(btnApply);
            btnApply.Y = y;
            btnApply.Width = 100;
            btnApply.Text = "Apply";
            AddChild(btnApply);
            btnApply.CenterOnParentHorizontally();
            btnApply.LeftClick += BtnApply_LeftClick;

            Height = btnApply.Bottom + Constants.UIEmptyBottomSpace;

            var closeButton = new EditorButton(WindowManager);
            closeButton.Name = "btnCloseX";
            closeButton.Width = Constants.UIButtonHeight;
            closeButton.Height = Constants.UIButtonHeight;
            closeButton.Text = "X";
            closeButton.X = Width - closeButton.Width;
            closeButton.Y = 0;
            AddChild(closeButton);
            closeButton.LeftClick += (s, e) => Hide();

            base.Initialize();
        }

        private void BtnApply_LeftClick(object sender, EventArgs e)
        {
            var terrainTypeGroups = new List<TerrainGeneratorTerrainTypeGroup>();

            for (int i = 0; i < terrainTypeTextBoxes.Length; i++)
            {
                string text = terrainTypeTextBoxes[i].Text.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                string[] parts = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var terrainTypes = new List<TerrainType>();
                for (int a = 0; a < parts.Length; a++)
                {
                    var terrainType = map.Rules.TerrainTypes.Find(tt => tt.ININame == parts[a]);
                    if (terrainType == null)
                    {
                        EditorMessageBox.Show(WindowManager, "Generator Config Error",
                            $"Specified terrain type '{ parts[a] }' does not exist!", MessageBoxButtons.OK);
                        return;
                    }
                    terrainTypes.Add(terrainType);
                }

                var terrainTypeGroup = new TerrainGeneratorTerrainTypeGroup(terrainTypes,
                    terrainTypeOpenChances[i].DoubleValue,
                    terrainTypeOccupiedChances[i].DoubleValue);

                terrainTypeGroups.Add(terrainTypeGroup);
            }

            var tileGroups = new List<TerrainGeneratorTileGroup>();

            for (int i = 0; i < tileSetSelectors.Length; i++)
            {
                var tileSet = (TileSet)tileSetSelectors[i].Tag;
                if (tileSet == null)
                    continue;

                List<int> tileIndexesInSet = null;
                string tileIndicesText = tileIndices[i].Text.Trim();
                if (!string.IsNullOrEmpty(tileIndicesText))
                {
                    string[] parts = tileIndicesText.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    tileIndexesInSet = parts.Select(str => Conversions.IntFromString(str, -1)).ToList();
                    int invalidElement = tileIndexesInSet.Find(index => index <= -1 || index >= tileSet.LoadedTileCount);

                    if (invalidElement != 0) // this can never be 0 if an invalid element exists, because each valid tileset has at least 1 tile
                    {
                        EditorMessageBox.Show(WindowManager, "Generator Config Error",
                            $"Tile with index '{ invalidElement }' does not exist in tile set '{ tileSet.SetName }'!", MessageBoxButtons.OK);
                        return;
                    }
                }

                var tileGroup = new TerrainGeneratorTileGroup(tileSet, tileIndexesInSet,
                    tileGroupOpenChances[i].DoubleValue,
                    tileGroupOccupiedChances[i].DoubleValue);

                tileGroups.Add(tileGroup);
            }

            TerrainGeneratorConfiguration = new TerrainGeneratorConfiguration(terrainTypeGroups, tileGroups);

            Hide();
        }

        private void TileSetDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            openedTileSetSelector.Tag = selectTileSetWindow.SelectedObject;

            if (selectTileSetWindow.SelectedObject == null)
                openedTileSetSelector.Text = string.Empty;
            else
                openedTileSetSelector.Text = selectTileSetWindow.SelectedObject.SetName;
        }

        private void SelTileSet_LeftClick(object sender, System.EventArgs e)
        {
            openedTileSetSelector = (EditorPopUpSelector)sender;
            selectTileSetWindow.Open((TileSet)openedTileSetSelector.Tag);
        }

        public void Open()
        {
            Show();

            if (TerrainGeneratorConfiguration == null)
            {
                // Generate default config
                // Just a reasonable example for now

                var terrainTypes = map.Rules.TerrainTypes;
                var tileSets = map.TheaterInstance.Theater.TileSets;

                var treeGroupTerrainTypes = terrainTypes.FindAll(tt => tt.ININame.StartsWith("TC0"));
                var singleTreeTerrainTypes = new List<TerrainType>();
                string[] conifers = new string[] { "T01", "T02", "T05", "T06", "T07", "T08", "T09", "T16" };
                Array.ForEach(conifers, c => singleTreeTerrainTypes.Add(terrainTypes.Find(tt => tt.ININame == c)));

                var treeGroups = new List<TerrainGeneratorTerrainTypeGroup>();
                treeGroups.Add(new TerrainGeneratorTerrainTypeGroup(treeGroupTerrainTypes, 0.125, 0.0));
                treeGroups.Add(new TerrainGeneratorTerrainTypeGroup(singleTreeTerrainTypes, 0.15, 0.15));

                var tileGroups = new List<TerrainGeneratorTileGroup>();
                tileGroups.Add(new TerrainGeneratorTileGroup(tileSets.Find(ts => ts.LoadedTileCount > 0 && ts.SetName == "Pebbles"), null, 0.3, 0.25));
                tileGroups.Add(new TerrainGeneratorTileGroup(tileSets.Find(ts => ts.LoadedTileCount > 0 && ts.SetName == "Small Rocks"), null, 0.05, 0.0));
                tileGroups.Add(new TerrainGeneratorTileGroup(tileSets.Find(ts => ts.LoadedTileCount > 0 && ts.SetName == "Debris/Dirt"), null, 0.02, 0.02));
                tileGroups.Add(new TerrainGeneratorTileGroup(tileSets.Find(ts => ts.LoadedTileCount > 0 && ts.SetName == "Tall Grass"), null, 0.6, 0.3));

                var config = new TerrainGeneratorConfiguration(treeGroups, tileGroups);
                LoadConfig(config);
            }
        }

        private void LoadConfig(TerrainGeneratorConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            for (int i = 0; i < configuration.TerrainTypeGroups.Count; i++)
            {
                var ttGroup = configuration.TerrainTypeGroups[i];

                terrainTypeTextBoxes[i].Text = string.Join(",", ttGroup.TerrainTypes.Select(tt => tt.ININame));
                terrainTypeOpenChances[i].DoubleValue = ttGroup.OpenChance;
                terrainTypeOccupiedChances[i].DoubleValue = ttGroup.OverlapChance;
            }

            for (int i = configuration.TerrainTypeGroups.Count; i < terrainTypeTextBoxes.Length; i++)
            {
                terrainTypeTextBoxes[i].Text = string.Empty;
                terrainTypeOpenChances[i].DoubleValue = 0.0;
                terrainTypeOccupiedChances[i].DoubleValue = 0.0;
            }

            for (int i = 0; i < configuration.TileGroups.Count; i++)
            {
                var tileGroup = configuration.TileGroups[i];

                tileSetSelectors[i].Text = tileGroup.TileSet.SetName;
                tileSetSelectors[i].Tag = tileGroup.TileSet;

                if (tileGroup.TileIndicesInSet == null)
                    tileIndices[i].Text = string.Empty;
                else
                    tileIndices[i].Text = string.Join(",", tileGroup.TileIndicesInSet.Select(index => index.ToString(CultureInfo.InvariantCulture)));

                tileGroupOpenChances[i].DoubleValue = tileGroup.OpenChance;
                tileGroupOccupiedChances[i].DoubleValue = tileGroup.OverlapChance;
            }

            for (int i = configuration.TileGroups.Count; i < tileSetSelectors.Length; i++)
            {
                tileSetSelectors[i].Text = string.Empty;
                tileSetSelectors[i].Tag = null;
                tileIndices[i].Text = string.Empty;
                tileGroupOpenChances[i].DoubleValue = 0.0;
                tileGroupOccupiedChances[i].DoubleValue = 0.0;
            }
        }
    }
}
