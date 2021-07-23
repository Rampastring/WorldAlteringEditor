using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Windows;

namespace TSMapEditor.UI.TopBar
{
    public class TopBarControlMenu : EditorPanel
    {
        public TopBarControlMenu(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics,
            EditorConfig editorConfig, EditorState editorState, TerrainPlacementAction terrainPlacementAction) : base(windowManager)
        {
            this.map = map;
            this.theaterGraphics = theaterGraphics;
            this.editorConfig = editorConfig;
            this.editorState = editorState;
            this.terrainPlacementAction = terrainPlacementAction;
        }

        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;
        private readonly EditorConfig editorConfig;
        private readonly EditorState editorState;
        private readonly TerrainPlacementAction terrainPlacementAction;

        private XNADropDown ddBrushSize;
        private XNACheckBox chkAutoLat;

        public override void Initialize()
        {
            Name = nameof(TopBarControlMenu);

            var lblBrushSize = new XNALabel(WindowManager);
            lblBrushSize.Name = nameof(lblBrushSize);
            lblBrushSize.X = Constants.UIEmptySideSpace;
            lblBrushSize.Y = Constants.UIEmptyTopSpace / 2;
            lblBrushSize.Text = "Brush size:";
            AddChild(lblBrushSize);

            ddBrushSize = new XNADropDown(WindowManager);
            ddBrushSize.Name = nameof(ddBrushSize);
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

            int prevRight = ddBrushSize.Right;

            for (int i = 0; i < map.TheaterInstance.Theater.LATGrounds.Count; i++)
            {
                LATGround autoLATGround = map.TheaterInstance.Theater.LATGrounds[i];

                var btn = new EditorButton(WindowManager);
                btn.Name = "btn" + autoLATGround.GroundTileSet.SetName;
                btn.X = prevRight + Constants.UIHorizontalSpacing;
                btn.Y = ddBrushSize.Y;
                btn.Width = 20;
                btn.Text = autoLATGround.GroundTileSet.SetName.Substring(0, 2);
                btn.Tag = autoLATGround;
                btn.LeftClick += GroundButton_LeftClick;
                AddChild(btn);

                prevRight = btn.Right;
            }

            chkAutoLat = new XNACheckBox(WindowManager);
            chkAutoLat.Name = nameof(chkAutoLat);
            chkAutoLat.X = prevRight + Constants.UIHorizontalSpacing;
            chkAutoLat.Y = ddBrushSize.Y;
            chkAutoLat.Checked = editorState.AutoLATEnabled;
            chkAutoLat.Text = "Auto-LAT";
            AddChild(chkAutoLat);
            chkAutoLat.CheckedChanged += ChkAutoLat_CheckedChanged;

            KeyboardCommands.Instance.NextBrushSize.Triggered += NextBrushSize_Triggered;
            KeyboardCommands.Instance.PreviousBrushSize.Triggered += PreviousBrushSize_Triggered;

            base.Initialize();

            editorState.AutoLATEnabledChanged += EditorState_AutoLATEnabledChanged;
        }

        private void GroundButton_LeftClick(object sender, EventArgs e)
        {
            var button = (EditorButton)sender;
            var latGround = button.Tag as LATGround;

            terrainPlacementAction.Tile = theaterGraphics.GetTileGraphics(latGround.GroundTileSet.StartTileIndex);
            editorState.CursorAction = terrainPlacementAction;
        }

        private void ChkAutoLat_CheckedChanged(object sender, EventArgs e)
        {
            chkAutoLat.CheckedChanged -= ChkAutoLat_CheckedChanged;
            editorState.AutoLATEnabled = !editorState.AutoLATEnabled;
            chkAutoLat.CheckedChanged += ChkAutoLat_CheckedChanged;
        }

        private void EditorState_AutoLATEnabledChanged(object sender, EventArgs e)
        {
            chkAutoLat.Checked = editorState.AutoLATEnabled;
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
