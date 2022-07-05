using System;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.TopBar
{
    public class TopBarControlMenu : EditorPanel
    {
        public TopBarControlMenu(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics,
            EditorConfig editorConfig, EditorState editorState,
            PlaceTerrainCursorAction terrainPlacementAction,
            PlaceWaypointCursorAction placeWaypointCursorAction,
            DeletionModeAction deletionModeAction) : base(windowManager)
        {
            this.map = map;
            this.theaterGraphics = theaterGraphics;
            this.editorConfig = editorConfig;
            this.editorState = editorState;
            this.terrainPlacementAction = terrainPlacementAction;
            this.placeWaypointCursorAction = placeWaypointCursorAction;
            this.deletionModeAction = deletionModeAction;
        }

        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;
        private readonly EditorConfig editorConfig;
        private readonly EditorState editorState;
        private readonly PlaceTerrainCursorAction terrainPlacementAction;
        private readonly PlaceWaypointCursorAction placeWaypointCursorAction;
        private readonly DeletionModeAction deletionModeAction;

        private XNADropDown ddBrushSize;
        private XNACheckBox chkAutoLat;
        private XNACheckBox chkOnlyPaintOnClearGround;
        private XNACheckBox chkDrawMapWideOverlay;

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

            var btnClearTerrain = new EditorButton(WindowManager);
            btnClearTerrain.Name = nameof(btnClearTerrain);
            btnClearTerrain.X = ddBrushSize.Right + Constants.UIHorizontalSpacing;
            btnClearTerrain.Y = ddBrushSize.Y;
            btnClearTerrain.Width = 50;
            btnClearTerrain.Text = "Clear";
            btnClearTerrain.LeftClick += BtnClearTerrain_LeftClick;
            AddChild(btnClearTerrain);

            int prevRight = btnClearTerrain.Right;

            for (int i = 0; i < map.TheaterInstance.Theater.LATGrounds.Count; i++)
            {
                LATGround autoLATGround = map.TheaterInstance.Theater.LATGrounds[i];

                // If we already have a button for this ground type, then skip it
                // The editor can automatically place the correct LAT variations
                // of a tile based on its surroundings
                bool alreadyExists = false;
                for (int j = i - 1; j > -1; j--)
                {
                    if (map.TheaterInstance.Theater.LATGrounds[j].GroundTileSet == autoLATGround.GroundTileSet)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (alreadyExists)
                    continue;

                var btn = new EditorButton(WindowManager);
                btn.Name = "btn" + autoLATGround.GroundTileSet.SetName;
                btn.X = prevRight + Constants.UIHorizontalSpacing;
                btn.Y = ddBrushSize.Y;
                btn.Width = 60;
                btn.Text = autoLATGround.DisplayName;
                btn.Tag = autoLATGround;
                btn.LeftClick += GroundButton_LeftClick;
                AddChild(btn);

                var toolTip = new ToolTip(WindowManager, btn);
                toolTip.Text = autoLATGround.GroundTileSet.SetName;
                toolTip.ToolTipDelay = 500;

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

            chkOnlyPaintOnClearGround = new XNACheckBox(WindowManager);
            chkOnlyPaintOnClearGround.Name = nameof(chkOnlyPaintOnClearGround);
            chkOnlyPaintOnClearGround.X = chkAutoLat.X;
            chkOnlyPaintOnClearGround.Y = chkAutoLat.Bottom + Constants.UIVerticalSpacing;
            chkOnlyPaintOnClearGround.Checked = editorState.OnlyPaintOnClearGround;
            chkOnlyPaintOnClearGround.Text = "Only Paint on Clear";
            AddChild(chkOnlyPaintOnClearGround);
            chkOnlyPaintOnClearGround.CheckedChanged += ChkOnlyPaintOnClearGround_CheckedChanged;

            if (editorState.MapWideOverlayExists)
            {
                chkDrawMapWideOverlay = new XNACheckBox(WindowManager);
                chkDrawMapWideOverlay.Name = nameof(chkDrawMapWideOverlay);
                chkDrawMapWideOverlay.X = chkAutoLat.X;
                chkDrawMapWideOverlay.Y = chkOnlyPaintOnClearGround.Bottom + Constants.UIVerticalSpacing;
                chkDrawMapWideOverlay.Checked = editorState.DrawMapWideOverlay;
                chkDrawMapWideOverlay.Text = "Draw Map-Wide Overlay";
                AddChild(chkDrawMapWideOverlay);
                chkDrawMapWideOverlay.CheckedChanged += ChkDrawMapWideOverlay_CheckedChanged;
            }

            Width = chkOnlyPaintOnClearGround.Right + Constants.UIEmptySideSpace;

            var btnPlaceWaypoint = new EditorButton(WindowManager);
            btnPlaceWaypoint.Name = nameof(btnPlaceWaypoint);
            btnPlaceWaypoint.X = Constants.UIEmptySideSpace;
            btnPlaceWaypoint.Y = ddBrushSize.Bottom + Constants.UIVerticalSpacing;
            btnPlaceWaypoint.Width = 120;
            btnPlaceWaypoint.Text = "Place Waypoint";
            btnPlaceWaypoint.LeftClick += BtnPlaceWaypoint_LeftClick;
            AddChild(btnPlaceWaypoint);

            var btnDeletionMode = new EditorButton(WindowManager);
            btnDeletionMode.Name = nameof(btnDeletionMode);
            btnDeletionMode.X = btnPlaceWaypoint.Right + Constants.UIHorizontalSpacing;
            btnDeletionMode.Y = btnPlaceWaypoint.Y;
            btnDeletionMode.Width = btnPlaceWaypoint.Width;
            btnDeletionMode.Text = "Deletion Mode";
            btnDeletionMode.LeftClick += BtnDeletionMode_LeftClick;
            AddChild(btnDeletionMode);

            Height = btnPlaceWaypoint.Bottom + Constants.UIEmptyBottomSpace;

            KeyboardCommands.Instance.NextBrushSize.Triggered += NextBrushSize_Triggered;
            KeyboardCommands.Instance.PreviousBrushSize.Triggered += PreviousBrushSize_Triggered;
            KeyboardCommands.Instance.ToggleAutoLAT.Triggered += ToggleAutoLAT_Triggered;
            KeyboardCommands.Instance.ToggleMapWideOverlay.Triggered += (s, e) => { if (editorState.MapWideOverlayExists) chkDrawMapWideOverlay.Checked = !chkDrawMapWideOverlay.Checked; };

            base.Initialize();

            editorState.AutoLATEnabledChanged += EditorState_AutoLATEnabledChanged;
            editorState.OnlyPaintOnClearGroundChanged += EditorState_OnlyPaintOnClearGroundChanged;
            editorState.DrawMapWideOverlayChanged += EditorState_DrawMapWideOverlayChanged;
            editorState.BrushSizeChanged += EditorState_BrushSizeChanged;
        }

        private void EditorState_DrawMapWideOverlayChanged(object sender, EventArgs e)
        {
            chkDrawMapWideOverlay.Checked = editorState.DrawMapWideOverlay;
        }

        private void ChkDrawMapWideOverlay_CheckedChanged(object sender, EventArgs e)
        {
            editorState.DrawMapWideOverlay = chkDrawMapWideOverlay.Checked;
        }

        private void EditorState_BrushSizeChanged(object sender, EventArgs e)
        {
            ddBrushSize.SelectedIndex = map.EditorConfig.BrushSizes.FindIndex(bs => bs == editorState.BrushSize);
        }

        private void BtnDeletionMode_LeftClick(object sender, EventArgs e)
        {
            editorState.CursorAction = deletionModeAction;
        }

        private void BtnPlaceWaypoint_LeftClick(object sender, EventArgs e)
        {
            editorState.CursorAction = placeWaypointCursorAction;
        }

        private void BtnClearTerrain_LeftClick(object sender, EventArgs e)
        {
            terrainPlacementAction.Tile = theaterGraphics.GetTileGraphics(0);
            editorState.CursorAction = terrainPlacementAction;
        }

        private void GroundButton_LeftClick(object sender, EventArgs e)
        {
            var button = (EditorButton)sender;
            var latGround = button.Tag as LATGround;

            terrainPlacementAction.Tile = theaterGraphics.GetTileGraphics(latGround.GroundTileSet.StartTileIndex);
            editorState.CursorAction = terrainPlacementAction;
        }

        private void ToggleAutoLAT_Triggered(object sender, EventArgs e)
        {
            chkAutoLat.Checked = !chkAutoLat.Checked;
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

        private void ChkOnlyPaintOnClearGround_CheckedChanged(object sender, EventArgs e)
        {
            chkOnlyPaintOnClearGround.CheckedChanged -= ChkOnlyPaintOnClearGround_CheckedChanged;
            editorState.OnlyPaintOnClearGround = !editorState.OnlyPaintOnClearGround;
            chkOnlyPaintOnClearGround.CheckedChanged += ChkOnlyPaintOnClearGround_CheckedChanged;
        }

        private void EditorState_OnlyPaintOnClearGroundChanged(object sender, EventArgs e)
        {
            chkOnlyPaintOnClearGround.Checked = editorState.OnlyPaintOnClearGround;
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
