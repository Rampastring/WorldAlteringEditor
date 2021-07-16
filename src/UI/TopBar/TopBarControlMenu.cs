using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.TopBar
{
    public class TopBarControlMenu : EditorPanel
    {
        public TopBarControlMenu(WindowManager windowManager, Map map, EditorConfig editorConfig, EditorState editorState) : base(windowManager)
        {
            this.map = map;
            this.editorConfig = editorConfig;
            this.editorState = editorState;
        }

        private readonly Map map;
        private readonly EditorConfig editorConfig;
        private readonly EditorState editorState;

        private XNADropDown ddBrushSize;

        public override void Initialize()
        {
            Name = nameof(TopBarControlMenu);

            var lblBrushSize = new XNALabel(WindowManager);
            lblBrushSize.X = Constants.UIEmptySideSpace;
            lblBrushSize.Y = Constants.UIEmptyTopSpace / 2;
            lblBrushSize.Text = "Brush size:";
            AddChild(lblBrushSize);

            ddBrushSize = new XNADropDown(WindowManager);
            ddBrushSize.X = lblBrushSize.Right + Constants.UIHorizontalSpacing;
            ddBrushSize.Y = lblBrushSize.Y - 1;
            ddBrushSize.Width = 60;
            AddChild(ddBrushSize);
            foreach (var brushSize in editorConfig.BrushSizes)
            {
                ddBrushSize.AddItem(brushSize.Width + "x" + brushSize.Height);
            }
            ddBrushSize.SelectedIndexChanged += DdBrushSize_SelectedIndexChanged;
            ddBrushSize.SelectedIndex = 0;
            
            KeyboardCommands.Instance.NextBrushSize.Triggered += NextBrushSize_Triggered;
            KeyboardCommands.Instance.PreviousBrushSize.Triggered += PreviousBrushSize_Triggered;

            base.Initialize();
        }

        private void DdBrushSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            editorState.BrushSize = editorConfig.BrushSizes[ddBrushSize.SelectedIndex];
        }

        private void PreviousBrushSize_Triggered(object sender, EventArgs e)
        {
            if (ddBrushSize.SelectedIndex < 1)
                return;

            ddBrushSize.SelectedIndex--;
        }

        private void NextBrushSize_Triggered(object sender, EventArgs e)
        {
            if (ddBrushSize.SelectedIndex >= ddBrushSize.Items.Count - 1)
                return;

            ddBrushSize.SelectedIndex++;
        }
    }
}
