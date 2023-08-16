using Rampastring.Tools;
using System;
using System.Collections.Generic;
using Rampastring.XNAUI.Input;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows pasting previously copied terrain.
    /// </summary>
    public class PasteTerrainCursorAction : CursorAction
    {
        public PasteTerrainCursorAction(ICursorActionTarget cursorActionTarget, RKeyboard keyboard) : base(cursorActionTarget)
        {
            this.keyboard = keyboard;
        }

        public override string GetName() => "Paste Copied Terrain";

        struct OriginalOverlayInfo
        {
            public Point2D CellCoords;
            public OverlayType OverlayType;
            public int FrameIndex;

            public OriginalOverlayInfo(Point2D cellCoords, OverlayType overlayType, int frameIndex)
            {
                CellCoords = cellCoords;
                OverlayType = overlayType;
                FrameIndex = frameIndex;
            }
        }


        private CopiedMapData copiedMapData;

        private List<OriginalOverlayInfo> originalOverlay = new List<OriginalOverlayInfo>();

        private RKeyboard keyboard;

        public override void OnActionEnter()
        {
            base.OnActionEnter();

            if (!System.Windows.Forms.Clipboard.ContainsData(Constants.ClipboardMapDataFormatValue))
            {
                Logger.Log(nameof(PasteTerrainCursorAction) + ": invalid clipboard data format, exiting action");
                ExitAction();
                return;
            }

            byte[] data = (byte[])System.Windows.Forms.Clipboard.GetData(Constants.ClipboardMapDataFormatValue);

            try
            {
                copiedMapData = new CopiedMapData();
                copiedMapData.Deserialize(data);
            }
            catch (CopiedMapDataSerializationException ex)
            {
                Logger.Log(nameof(PasteTerrainCursorAction) + ": exception when decoding data from clipboard, exiting action. Message: " + ex.Message);
                ExitAction();
            }
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            originalOverlay.Clear();

            int maxOffset = 0;

            foreach (var entry in copiedMapData.CopiedMapEntries)
            {
                maxOffset = Math.Max(maxOffset, Math.Max(Math.Abs(entry.Offset.X), Math.Abs(entry.Offset.Y)));

                MapTile cell = CursorActionTarget.Map.GetTile(cellCoords + entry.Offset);
                if (cell == null)
                    continue;

                if (entry.EntryType == CopiedEntryType.Terrain)
                {
                    var terrainEntry = entry as CopiedTerrainEntry;
                    cell.PreviewTileImage = CursorActionTarget.TheaterGraphics.GetTileGraphics(terrainEntry.TileIndex, 0);
                    cell.PreviewSubTileIndex = terrainEntry.SubTileIndex;
                    cell.PreviewLevel = cell.Level;
                }
                /*else if (entry.EntryType == CopiedEntryType.Overlay)
                {
                    var overlayEntry = entry as CopiedOverlayEntry;

                    // Store original overlay info
                    if (cell.Overlay != null)
                        originalOverlay.Add(new OriginalOverlayInfo(cell.CoordsToPoint(), cell.Overlay.OverlayType, cell.Overlay.FrameIndex));
                    else
                        originalOverlay.Add(new OriginalOverlayInfo(cell.CoordsToPoint(), null, Constants.NO_OVERLAY));

                    // Apply new overlay info
                    if (cell.Overlay == null)
                    {
                        // Creating new object instances each frame is not very performance-friendly, we might want to revise this later...
                        cell.Overlay = new Overlay()
                        {
                            Position = cell.CoordsToPoint(),
                            OverlayType = overlayEntry.O,
                            FrameIndex = 0
                        };
                    }
                    else
                    {
                        tile.Overlay.OverlayType = OverlayType;
                        tile.Overlay.FrameIndex = 0;
                    }

                    if (FrameIndex == null)
                        tile.Overlay.FrameIndex = CursorActionTarget.Map.GetOverlayFrameIndex(tile.CoordsToPoint());
                    else
                        tile.Overlay.FrameIndex = FrameIndex.Value;
                }*/
            }

            CursorActionTarget.AddRefreshPoint(cellCoords, maxOffset);
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            int maxOffset = 0;

            foreach (var copiedTerrain in copiedMapData.CopiedMapEntries)
            {
                if (copiedTerrain.EntryType != CopiedEntryType.Terrain)
                    continue;

                maxOffset = Math.Max(maxOffset, Math.Max(Math.Abs(copiedTerrain.Offset.X), Math.Abs(copiedTerrain.Offset.Y)));

                MapTile cell = CursorActionTarget.Map.GetTile(cellCoords + copiedTerrain.Offset);
                if (cell == null)
                    continue;

                cell.PreviewTileImage = null;
            }

            CursorActionTarget.AddRefreshPoint(cellCoords, maxOffset);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            if (CursorActionTarget.Map.GetTile(cellCoords) == null)
                return;

            bool allowOverlap = KeyboardCommands.Instance.OverlapObjects.AreKeysOrModifiersDown(keyboard);
            
            var mutation = new PasteTerrainMutation(CursorActionTarget.MutationTarget, copiedMapData, cellCoords, allowOverlap);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
