using Microsoft.Xna.Framework.Input;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.Sidebar
{
    public class EditorSidebar : EditorPanel
    {
        public EditorSidebar(WindowManager windowManager, EditorState editorState, Map map,
            TheaterGraphics theaterGraphics, ICursorActionTarget cursorActionTarget,
            OverlayPlacementAction overlayPlacementAction) : base(windowManager)
        {
            this.editorState = editorState;
            this.map = map;
            this.theaterGraphics = theaterGraphics;
            this.cursorActionTarget = cursorActionTarget;
            this.overlayPlacementAction = overlayPlacementAction;
        }

        private readonly EditorState editorState;
        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;
        private readonly OverlayPlacementAction overlayPlacementAction;

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
            lbSelection.FontIndex = Constants.UIBoldFont;

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

            var buildingListPanel = new BuildingListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            buildingListPanel.Name = nameof(buildingListPanel);
            InitPanel(buildingListPanel);

            var unitListPanel = new UnitListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget, false);
            unitListPanel.Name = nameof(unitListPanel);
            InitPanel(unitListPanel);

            var navalUnitListPanel = new UnitListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget, true);
            navalUnitListPanel.Name = nameof(navalUnitListPanel);
            InitPanel(navalUnitListPanel);

            var infantryListPanel = new InfantryListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            infantryListPanel.Name = nameof(infantryListPanel);
            InitPanel(infantryListPanel);

            var terrainObjectListPanel = new TerrainObjectListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            terrainObjectListPanel.Name = nameof(terrainObjectListPanel);
            InitPanel(terrainObjectListPanel);

            var overlayListPanel = new OverlayListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget, overlayPlacementAction);
            overlayListPanel.Name = nameof(overlayListPanel);
            InitPanel(overlayListPanel);

            var smudgeListPanel = new SmudgeListPanel(WindowManager, editorState, map, theaterGraphics, cursorActionTarget);
            smudgeListPanel.Name = nameof(smudgeListPanel);
            InitPanel(smudgeListPanel);

            modePanels = new XNAPanel[]
            {
                aircraftListPanel,
                buildingListPanel,
                unitListPanel,
                navalUnitListPanel,
                infantryListPanel,
                terrainObjectListPanel,
                overlayListPanel,
                smudgeListPanel
            };
            lbSelection.SelectedIndexChanged += LbSelection_SelectedIndexChanged;
            lbSelection.SelectedIndex = 0;

            base.Initialize();

            KeyboardCommands.Instance.AircraftMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 0;
            KeyboardCommands.Instance.BuildingMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 1;
            KeyboardCommands.Instance.VehicleMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 2;
            KeyboardCommands.Instance.NavalMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 3;
            KeyboardCommands.Instance.InfantryMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 4;
            KeyboardCommands.Instance.TerrainObjectMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 5;
            KeyboardCommands.Instance.OverlayMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 6;
            KeyboardCommands.Instance.SmudgeMenu.Triggered += (s, e) => lbSelection.SelectedIndex = 7;

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
