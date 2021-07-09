using Microsoft.Xna.Framework.Input;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class EditorSidebar : EditorPanel
    {
        public EditorSidebar(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics, ICursorActionTarget cursorActionTarget) : base(windowManager)
        {
            this.editorState = editorState;
            this.map = map;
            this.theaterGraphics = theaterGraphics;
            this.cursorActionTarget = cursorActionTarget;
        }

        private readonly EditorState editorState;
        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;

        private XNAListBox lbSelection;

        private XNAPanel[] modePanels;
        private XNAPanel activePanel;

        private ICursorActionTarget cursorActionTarget;

        public override void Initialize()
        {
            Name = nameof(EditorSidebar);

            lbSelection = new XNAListBox(WindowManager);
            lbSelection.Name = nameof(lbSelection);
            lbSelection.X = 0;
            lbSelection.Y = 0;
            lbSelection.Width = Width;

            for (int i = 1; i < (int)SidebarMode.SidebarModeCount; i++)
            {
                SidebarMode sidebarMode = (SidebarMode)i;
                lbSelection.AddItem(new XNAListBoxItem() { Text = sidebarMode.ToString(), Tag = sidebarMode });
            }

            lbSelection.Height = lbSelection.Items.Count * lbSelection.LineHeight + 5;
            AddChild(lbSelection);
            lbSelection.EnableScrollbar = false;

            var aircraftListPanel = new AircraftListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            aircraftListPanel.Name = nameof(aircraftListPanel);
            InitPanel(aircraftListPanel);

            var buildingListPanel = new BuildingListPanel(WindowManager, editorState, map, theaterGraphics);
            buildingListPanel.Name = nameof(buildingListPanel);
            InitPanel(buildingListPanel);

            var unitListPanel = new UnitListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            unitListPanel.Name = nameof(unitListPanel);
            InitPanel(unitListPanel);

            var infantryListPanel = new InfantryListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            infantryListPanel.Name = nameof(infantryListPanel);
            InitPanel(infantryListPanel);

            var terrainObjectListPanel = new TerrainObjectListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            terrainObjectListPanel.Name = nameof(terrainObjectListPanel);
            InitPanel(terrainObjectListPanel);

            var overlayListPanel = new OverlayListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            overlayListPanel.Name = nameof(overlayListPanel);
            InitPanel(overlayListPanel);

            modePanels = new XNAPanel[]
            {
                aircraftListPanel,
                buildingListPanel,
                unitListPanel,
                infantryListPanel,
                terrainObjectListPanel,
                overlayListPanel
            };
            lbSelection.SelectedIndexChanged += LbSelection_SelectedIndexChanged;
            lbSelection.SelectedIndex = 0;

            base.Initialize();

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
        }

        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            if (!WindowManager.HasFocus)
                return;

            if (e.PressedKey == Keys.F && Keyboard.IsCtrlHeldDown())
            {
                if (activePanel != null)
                {
                    if (activePanel is ISearchBoxContainer searchBoxContainer)
                        WindowManager.SelectedControl = searchBoxContainer.SearchBox;
                }
            }
        }

        private void LbSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var panel in modePanels)
            {
                if (panel != null)
                    panel.Disable();
            }

            activePanel = null;
            int selectedIndex = lbSelection.SelectedIndex;

            if (selectedIndex > -1)
            {
                if (modePanels[selectedIndex] != null)
                    modePanels[selectedIndex].Enable();

                activePanel = modePanels[selectedIndex];
            }
        }

        private void InitPanel(XNAPanel panel)
        {
            panel.Y = lbSelection.Bottom;
            panel.Height = Height - panel.Y;
            panel.Width = Width;
            AddChild(panel);
        }
    }
}
