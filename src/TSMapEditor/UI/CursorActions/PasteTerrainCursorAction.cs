using Rampastring.Tools;
using System;
using System.Collections.Generic;
using Rampastring.XNAUI.Input;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;
using Rampastring.XNAUI;
using Microsoft.Xna.Framework;

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

        struct OriginalSmudgeInfo
        {
            public Point2D CellCoords;
            public SmudgeType SmudgeType;


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
            MapTile originCell = MutationTarget.Map.GetTile(cellCoords);
            int originLevel = originCell?.Level ?? -1;

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
                    cell.PreviewLevel = Math.Min(Constants.MaxMapHeightLevel, originLevel + terrainEntry.HeightOffset);
                }
                else if (entry.EntryType == CopiedEntryType.Overlay)
                {
                    var overlayEntry = entry as CopiedOverlayEntry;

                    // Store original overlay info
                    if (cell.Overlay != null)
                        originalOverlay.Add(new OriginalOverlayInfo(cell.CoordsToPoint(), cell.Overlay.OverlayType, cell.Overlay.FrameIndex));
                    else
                        originalOverlay.Add(new OriginalOverlayInfo(cell.CoordsToPoint(), null, Constants.NO_OVERLAY));

                    var overlayType = Map.Rules.OverlayTypes.Find(ot => ot.ININame == overlayEntry.OverlayTypeName);
                    if (overlayType == null) 
                    {
                        continue;
                    }

                    // Apply new overlay info
                    if (cell.Overlay == null)
                    {
                        // Creating new object instances each frame is not very performance-friendly, we might want to revise this later...
                        cell.Overlay = new Overlay()
                        {
                            Position = cell.CoordsToPoint(),
                            OverlayType = overlayType,
                            FrameIndex = overlayEntry.FrameIndex
                        };
                    }
                    else
                    {
                        cell.Overlay.OverlayType = overlayType;
                        cell.Overlay.FrameIndex = overlayEntry.FrameIndex;
                    }
                }
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

            foreach (var originalOverlayEntry in originalOverlay)
            {
                MapTile cell = Map.GetTile(originalOverlayEntry.CellCoords);

                if (originalOverlayEntry.OverlayType == null)
                {
                    cell.Overlay = null;
                }
                else
                {
                    cell.Overlay.OverlayType = originalOverlayEntry.OverlayType;
                    cell.Overlay.FrameIndex = originalOverlayEntry.FrameIndex;
                }
            }

            CursorActionTarget.AddRefreshPoint(cellCoords, maxOffset);
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            int startY = cellCoords.Y;
            int endY = cellCoords.Y + copiedMapData.Height;
            int startX = cellCoords.X;
            int endX = cellCoords.X + copiedMapData.Width;

            Func<Point2D, Map, Point2D> func = Is2DMode ? CellMath.CellTopLeftPointFromCellCoords : CellMath.CellTopLeftPointFromCellCoords_3D;

            Point2D startPoint = func(new Point2D(startX, startY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX / 2, 0);
            Point2D endPoint = func(new Point2D(endX, endY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX / 2, Constants.CellSizeY);
            Point2D corner1 = func(new Point2D(startX, endY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(0, Constants.CellSizeY / 2);
            Point2D corner2 = func(new Point2D(endX, startY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX, Constants.CellSizeY / 2);

            startPoint = startPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            endPoint = endPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            corner1 = corner1.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            corner2 = corner2.ScaleBy(CursorActionTarget.Camera.ZoomLevel);

            Color lineColor = Color.Orange;
            int thickness = 2;
            Renderer.DrawLine(startPoint.ToXNAVector(), corner1.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(startPoint.ToXNAVector(), corner2.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner1.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner2.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
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
