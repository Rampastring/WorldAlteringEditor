using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
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

            modePanels = new XNAPanel[lbSelection.Items.Count];

            var unitListPanel = new UnitListPanel(WindowManager, editorState, map, theaterGraphics);
            unitListPanel.Name = nameof(unitListPanel);
            unitListPanel.Y = lbSelection.Bottom;
            unitListPanel.Height = Height - unitListPanel.Y;
            unitListPanel.Width = Width;
            AddChild(unitListPanel);

            base.Initialize();
        }


    }
}
