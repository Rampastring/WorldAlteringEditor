using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.TopBar
{
    public class EditorControlsPanel : INItializableWindow
    {
        public EditorControlsPanel(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics,
            EditorConfig editorConfig, EditorState editorState,
            PlaceTerrainCursorAction terrainPlacementAction,
            PlaceWaypointCursorAction placeWaypointCursorAction,
            ICursorActionTarget cursorActionTarget) : base(windowManager)
        {
            this.map = map;
            this.theaterGraphics = theaterGraphics;
            this.editorConfig = editorConfig;
            this.editorState = editorState;
            this.terrainPlacementAction = terrainPlacementAction;
            this.placeWaypointCursorAction = placeWaypointCursorAction;

            deletionModeCursorAction = new DeletionModeCursorAction(cursorActionTarget);
            raiseCellsCursorAction = new RaiseCellsCursorAction(cursorActionTarget);
            lowerCellsCursorAction = new LowerCellsCursorAction(cursorActionTarget);
        }

        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;
        private readonly EditorConfig editorConfig;
        private readonly EditorState editorState;
        private readonly PlaceTerrainCursorAction terrainPlacementAction;
        private readonly PlaceWaypointCursorAction placeWaypointCursorAction;
        private readonly DeletionModeCursorAction deletionModeCursorAction;
        private readonly RaiseCellsCursorAction raiseCellsCursorAction;
        private readonly LowerCellsCursorAction lowerCellsCursorAction;

        private XNADropDown ddBrushSize;
        private XNACheckBox chkAutoLAT;
        private XNACheckBox chkOnlyPaintOnClearGround;
        private XNACheckBox chkDrawMapWideOverlay;

        public override void Initialize()
        {
            SubDirectory = string.Empty;
            Name = nameof(EditorControlsPanel);
            base.Initialize();

            ddBrushSize = FindChild<XNADropDown>(nameof(ddBrushSize));
            foreach (var brushSize in editorConfig.BrushSizes)
            {
                ddBrushSize.AddItem(brushSize.Width + "x" + brushSize.Height);
            }
            ddBrushSize.SelectedIndexChanged += DdBrushSize_SelectedIndexChanged;
            ddBrushSize.SelectedIndex = 0;

            chkAutoLAT = FindChild<XNACheckBox>(nameof(chkAutoLAT));
            chkAutoLAT.CheckedChanged += ChkAutoLat_CheckedChanged;

            chkOnlyPaintOnClearGround = FindChild<XNACheckBox>(nameof(chkOnlyPaintOnClearGround));
            chkOnlyPaintOnClearGround.CheckedChanged += ChkOnlyPaintOnClearGround_CheckedChanged;

            chkDrawMapWideOverlay = FindChild<XNACheckBox>(nameof(chkDrawMapWideOverlay));
            chkDrawMapWideOverlay.CheckedChanged += ChkDrawMapWideOverlay_CheckedChanged;
            if (!editorState.MapWideOverlayExists)
                chkDrawMapWideOverlay.Disable();

            FindChild<EditorButton>("btnPlaceWaypoint").LeftClick += (s, e) => editorState.CursorAction = placeWaypointCursorAction;
            FindChild<EditorButton>("btnDeletionMode").LeftClick += (s, e) => editorState.CursorAction = deletionModeCursorAction;

            var btnRaiseCells = FindChild<EditorButton>("btnRaiseCells", true);
            if (btnRaiseCells != null)
                btnRaiseCells.LeftClick += (s, e) => editorState.CursorAction = raiseCellsCursorAction;

            var btnLowerCells = FindChild<EditorButton>("btnLowerCells", true);
            if (btnLowerCells != null)
                btnLowerCells.LeftClick += (s, e) => editorState.CursorAction = lowerCellsCursorAction;

            KeyboardCommands.Instance.NextBrushSize.Triggered += NextBrushSize_Triggered;
            KeyboardCommands.Instance.PreviousBrushSize.Triggered += PreviousBrushSize_Triggered;
            KeyboardCommands.Instance.ToggleAutoLAT.Triggered += ToggleAutoLAT_Triggered;
            KeyboardCommands.Instance.ToggleMapWideOverlay.Triggered += (s, e) => { if (editorState.MapWideOverlayExists) chkDrawMapWideOverlay.Checked = !chkDrawMapWideOverlay.Checked; };

            editorState.AutoLATEnabledChanged += (s, e) => chkAutoLAT.Checked = editorState.AutoLATEnabled;
            editorState.OnlyPaintOnClearGroundChanged += (s, e) => chkOnlyPaintOnClearGround.Checked = editorState.OnlyPaintOnClearGround;
            editorState.DrawMapWideOverlayChanged += (s, e) => chkDrawMapWideOverlay.Checked = editorState.DrawMapWideOverlay;
            editorState.BrushSizeChanged += (s, e) => ddBrushSize.SelectedIndex = map.EditorConfig.BrushSizes.FindIndex(bs => bs == editorState.BrushSize);

            InitLATPanel();
        }

        private void InitLATPanel()
        {
            var latPanel = FindChild<XNAPanel>("LATPanel");

            var btnClearTerrain = new EditorButton(WindowManager);
            btnClearTerrain.Name = nameof(btnClearTerrain);
            btnClearTerrain.X = 0;
            btnClearTerrain.Y = 0;
            btnClearTerrain.Width = Constants.CellSizeX + Constants.UIHorizontalSpacing * 2;
            btnClearTerrain.Height = Constants.CellSizeY + 2;
            btnClearTerrain.ExtraTexture = theaterGraphics.GetTileGraphics(0).TMPImages[0].Texture;
            btnClearTerrain.LeftClick += (s, e) => EnterLATPlacementMode(0);
            latPanel.AddChild(btnClearTerrain);
            var clearToolTip = new ToolTip(WindowManager, btnClearTerrain);
            clearToolTip.Text = "Clear";
            clearToolTip.ToolTipDelay = 0;

            int prevRight = btnClearTerrain.Right;
            int y = btnClearTerrain.Y;

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
                btn.Y = y;
                btn.Width = btnClearTerrain.Width;
                btn.Height = btnClearTerrain.Height;
                btn.ExtraTexture = theaterGraphics.GetTileGraphics(autoLATGround.GroundTileSet.StartTileIndex).TMPImages[0].Texture;
                btn.Tag = autoLATGround;
                btn.LeftClick += (s, e) => EnterLATPlacementMode(autoLATGround.GroundTileSet.StartTileIndex);
                latPanel.AddChild(btn);

                if (btn.Right > latPanel.Right)
                {
                    btn.X = btnClearTerrain.X;
                    prevRight = btn.Right;

                    y += btnClearTerrain.Height + Constants.UIVerticalSpacing;
                    btn.Y = y;
                }

                var toolTip = new ToolTip(WindowManager, btn);
                string[] allBases = map.TheaterInstance.Theater.LATGrounds.FindAll(lg => lg.GroundTileSet == autoLATGround.GroundTileSet).Select(lg =>
                {
                    if (lg.BaseTileSet == null)
                        return "Clear";

                    return lg.BaseTileSet.SetName;
                }).ToArray();

                toolTip.Text = $"{autoLATGround.GroundTileSet.SetName} (placed on top of {string.Join(" or ", allBases)})";

                toolTip.ToolTipDelay = 0;

                prevRight = btn.Right;
            }
        }

        private void EnterLATPlacementMode(int tileIndex)
        {
            terrainPlacementAction.Tile = theaterGraphics.GetTileGraphics(tileIndex);
            editorState.CursorAction = terrainPlacementAction;
        }

        private void DdBrushSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            editorState.BrushSize = editorConfig.BrushSizes[ddBrushSize.SelectedIndex];
        }

        private void ChkAutoLat_CheckedChanged(object sender, EventArgs e)
        {
            chkAutoLAT.CheckedChanged -= ChkAutoLat_CheckedChanged;
            editorState.AutoLATEnabled = !editorState.AutoLATEnabled;
            chkAutoLAT.CheckedChanged += ChkAutoLat_CheckedChanged;
        }

        private void ChkOnlyPaintOnClearGround_CheckedChanged(object sender, EventArgs e)
        {
            chkOnlyPaintOnClearGround.CheckedChanged -= ChkOnlyPaintOnClearGround_CheckedChanged;
            editorState.OnlyPaintOnClearGround = !editorState.OnlyPaintOnClearGround;
            chkOnlyPaintOnClearGround.CheckedChanged += ChkOnlyPaintOnClearGround_CheckedChanged;
        }

        private void ChkDrawMapWideOverlay_CheckedChanged(object sender, EventArgs e)
        {
            editorState.DrawMapWideOverlay = chkDrawMapWideOverlay.Checked;
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

        private void ToggleAutoLAT_Triggered(object sender, EventArgs e)
        {
            chkAutoLAT.Checked = !chkAutoLAT.Checked;
        }
    }
}
