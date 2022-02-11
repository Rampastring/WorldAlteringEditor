using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows.TerrainGenerator
{
    /// <summary>
    /// A window that allows the user to configure the terrain generator (see <see cref="TerrainGeneratorConfiguration"/>).
    /// </summary>
    public class TerrainGeneratorConfigWindow : EditorWindow
    {
        public TerrainGeneratorConfigWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public TerrainGeneratorConfiguration TerrainGeneratorConfig { get; private set; }

        private TerrainGeneratorTerrainTypeGroupsPanel terrainTypeGroupsPanel;
        private TerrainGeneratorTileGroupsPanel tileGroupsPanel;
        private TerrainGeneratorOverlayGroupsPanel overlayGroupsPanel;

        private XNADropDown ddPresets;

        private XNAPanel[] panels;

        public override void Initialize()
        {
            Width = 800;
            Name = nameof(TerrainGeneratorConfigWindow);

            var lblHeader = new XNALabel(WindowManager);
            lblHeader.Name = nameof(lblHeader);
            lblHeader.FontIndex = Constants.UIBoldFont;
            lblHeader.Text = "TERRAIN GENERATOR CONFIGURATION";
            lblHeader.Y = Constants.UIEmptyTopSpace;
            AddChild(lblHeader);
            lblHeader.CenterOnParentHorizontally();

            var lblPresets = new XNALabel(WindowManager);
            lblPresets.Name = nameof(lblPresets);
            lblPresets.Y = lblHeader.Bottom + Constants.UIEmptyTopSpace;
            lblPresets.X = Constants.UIEmptySideSpace;
            lblPresets.Text = "Presets:";
            AddChild(lblPresets);

            ddPresets = new XNADropDown(WindowManager);
            ddPresets.Name = nameof(ddPresets);
            ddPresets.Width = 250;
            ddPresets.Y = lblPresets.Y - 1;
            ddPresets.X = lblPresets.Right + Constants.UIHorizontalSpacing;
            AddChild(ddPresets);
            ddPresets.SelectedIndexChanged += DdPresets_SelectedIndexChanged;
            InitPresets();

            var customUISettings = UISettings.ActiveSettings as CustomUISettings;

            const int tabWidth = 150;
            const int tabHeight = 24;

            var idleTexture = Helpers.CreateUITexture(GraphicsDevice, tabWidth, tabHeight,
                customUISettings.ButtonMainBackgroundColor,
                customUISettings.ButtonSecondaryBackgroundColor,
                customUISettings.ButtonTertiaryBackgroundColor);

            var selectedTexture = Helpers.CreateUITexture(GraphicsDevice, tabWidth, tabHeight,
                new Color(128, 128, 128, 196),
                new Color(128, 128, 128, 255), Color.White);

            var tabControl = new XNATabControl(WindowManager);
            tabControl.Name = nameof(tabControl);
            tabControl.X = Constants.UIEmptySideSpace;
            tabControl.Y = ddPresets.Bottom + Constants.UIVerticalSpacing;
            tabControl.Width = Width;
            tabControl.Height = idleTexture.Height;
            tabControl.FontIndex = Constants.UIBoldFont;
            tabControl.AddTab("Terrain Types", idleTexture, selectedTexture);
            tabControl.AddTab("Terrain Tiles", idleTexture, selectedTexture);
            tabControl.AddTab("Overlays", idleTexture, selectedTexture);
            tabControl.AddTab("Smudges (TODO)", idleTexture, selectedTexture, false);
            AddChild(tabControl);
            tabControl.SelectedIndexChanged += (s, e) => { HideAllPanels(); panels[tabControl.SelectedTab].Enable(); };

            panels = new XNAPanel[4];

            terrainTypeGroupsPanel = new TerrainGeneratorTerrainTypeGroupsPanel(WindowManager, map);
            terrainTypeGroupsPanel.Name = nameof(terrainTypeGroupsPanel);
            terrainTypeGroupsPanel.X = Constants.UIEmptySideSpace;
            terrainTypeGroupsPanel.Y = tabControl.Bottom + 1;
            terrainTypeGroupsPanel.Width = Width - Constants.UIEmptySideSpace * 2;
            terrainTypeGroupsPanel.Height = 600;
            AddChild(terrainTypeGroupsPanel);
            panels[0] = terrainTypeGroupsPanel;

            tileGroupsPanel = new TerrainGeneratorTileGroupsPanel(WindowManager, map);
            tileGroupsPanel.Name = nameof(tileGroupsPanel);
            tileGroupsPanel.X = terrainTypeGroupsPanel.X;
            tileGroupsPanel.Y = terrainTypeGroupsPanel.Y;
            tileGroupsPanel.Width = terrainTypeGroupsPanel.Width;
            tileGroupsPanel.Height = terrainTypeGroupsPanel.Height;
            AddChild(tileGroupsPanel);
            panels[1] = tileGroupsPanel;

            overlayGroupsPanel = new TerrainGeneratorOverlayGroupsPanel(WindowManager, map);
            overlayGroupsPanel.Name = nameof(overlayGroupsPanel);
            overlayGroupsPanel.X = terrainTypeGroupsPanel.X;
            overlayGroupsPanel.Y = terrainTypeGroupsPanel.Y;
            overlayGroupsPanel.Width = terrainTypeGroupsPanel.Width;
            overlayGroupsPanel.Height = terrainTypeGroupsPanel.Height;
            AddChild(overlayGroupsPanel);
            panels[2] = overlayGroupsPanel;

            HideAllPanels();

            tabControl.SelectedTab = 0;
            panels[0].Enable();

            var btnApply = new EditorButton(WindowManager);
            btnApply.Name = nameof(btnApply);
            btnApply.Y = terrainTypeGroupsPanel.Bottom + Constants.UIEmptyTopSpace;
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

        private void HideAllPanels()
        {
            for (int i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null)
                    panels[i].Disable();
            }
        }

        private void InitPresets()
        {
            ddPresets.Items.Clear();

            var presetsIni = new IniFile(Environment.CurrentDirectory + "/Config/TerrainGeneratorPresets.ini");
            foreach (string sectionName in presetsIni.GetSections())
            {
                var presetConfiguration = TerrainGeneratorConfiguration.FromConfigSection(presetsIni.GetSection(sectionName),
                    map.Rules.TerrainTypes,
                    map.TheaterInstance.Theater.TileSets,
                    map.Rules.OverlayTypes);

                if (presetConfiguration != null)
                    ddPresets.AddItem(new XNADropDownItem() { Text = presetConfiguration.Name, Tag = presetConfiguration });
            }
        }

        private void DdPresets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddPresets.SelectedIndex < 0)
                return;

            var config = ddPresets.SelectedItem.Tag as TerrainGeneratorConfiguration;
            LoadConfig(config);
            ddPresets.SelectedIndex = -1;
        }

        private void BtnApply_LeftClick(object sender, EventArgs e)
        {
            var terrainTypeGroups = terrainTypeGroupsPanel.GetTerrainTypeGroups();
            var tileGroups = tileGroupsPanel.GetTileGroups();
            var overlayGroups = overlayGroupsPanel.GetOverlayGroups();

            // One of the panels returning null means there's an error condition
            if (terrainTypeGroups == null || tileGroups == null || overlayGroups == null)
                return;

            TerrainGeneratorConfig = new TerrainGeneratorConfiguration("Customized Configuration", terrainTypeGroups, tileGroups, overlayGroups);

            Hide();
        }

        public void Open()
        {
            Show();

            if (TerrainGeneratorConfig == null)
            {
                // Load first preset as default config

                var config = ddPresets.Items[0].Tag as TerrainGeneratorConfiguration;
                LoadConfig(config);
            }
        }

        private void LoadConfig(TerrainGeneratorConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            terrainTypeGroupsPanel.LoadConfig(configuration);
            tileGroupsPanel.LoadConfig(configuration);
            overlayGroupsPanel.LoadConfig(configuration);
        }
    }
}
