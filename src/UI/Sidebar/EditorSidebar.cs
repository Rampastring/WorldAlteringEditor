using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class EditorSidebar : EditorPanel
    {
        public EditorSidebar(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.editorState = editorState;
            this.map = map;
            this.theaterGraphics = theaterGraphics;
        }

        private readonly EditorState editorState;
        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;

        private XNAListBox lbSelection;

        private XNAPanel[] modePanels;

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

            var aircraftListPanel = new AircraftListPanel(WindowManager, editorState, map, theaterGraphics);
            aircraftListPanel.Name = nameof(aircraftListPanel);
            InitPanel(aircraftListPanel);

            var unitListPanel = new UnitListPanel(WindowManager, editorState, map, theaterGraphics);
            unitListPanel.Name = nameof(unitListPanel);
            InitPanel(unitListPanel);

            var infantryListPanel = new InfantryListPanel(WindowManager, editorState, map, theaterGraphics);
            infantryListPanel.Name = nameof(infantryListPanel);
            InitPanel(infantryListPanel);

            modePanels = new XNAPanel[]
            {
                aircraftListPanel,
                null, // buildings
                unitListPanel,
                infantryListPanel, // infantry
                null, // terrain objects
                null // overlay
            };
            lbSelection.SelectedIndexChanged += LbSelection_SelectedIndexChanged;
            lbSelection.SelectedIndex = 0;

            base.Initialize();
        }

        private void LbSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var panel in modePanels)
            {
                if (panel != null)
                    panel.Disable();
            }

            if (lbSelection.SelectedIndex > -1)
            {
                if (modePanels[lbSelection.SelectedIndex] != null)
                    modePanels[lbSelection.SelectedIndex].Enable();
            }
        }

        private void InitPanel(ObjectListPanel panel)
        {
            panel.Y = lbSelection.Bottom;
            panel.Height = Height - panel.Y;
            panel.Width = Width;
            AddChild(panel);
        }
    }
}
