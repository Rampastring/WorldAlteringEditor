using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows copying terrain tiles.
    /// </summary>
    public class CopyTerrainCursorAction : CursorAction
    {
        public CopyTerrainCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public CopiedEntryType EntryTypes { get; set; }

        public Point2D? StartCellCoords { get; set; } = null;

        public override void LeftClick(Point2D cellCoords)
        {
            if (StartCellCoords == null)
            {
                StartCellCoords = cellCoords;
                return;
            }

            var copiedMapData = new CopiedMapData();

            Point2D startCellCoords = StartCellCoords.Value;
            int startY = Math.Min(cellCoords.Y, startCellCoords.Y);
            int endY = Math.Max(cellCoords.Y, startCellCoords.Y);
            int startX = Math.Min(cellCoords.X, startCellCoords.X);
            int endX = Math.Max(cellCoords.X, startCellCoords.X);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var offset = new Point2D(x - startX, y - startY);
                    MapTile cell = CursorActionTarget.Map.GetTile(x, y);
                    if (cell == null)
                        continue;

                    if ((EntryTypes & CopiedEntryType.Terrain) == CopiedEntryType.Terrain)
                    {
                        copiedMapData.CopiedMapEntries.Add(new CopiedTerrainEntry(offset, cell.TileIndex, cell.SubTileIndex));
                    }
                    
                    if ((EntryTypes & CopiedEntryType.Overlay) == CopiedEntryType.Overlay)
                    {
                        if (cell.Overlay != null)
                            copiedMapData.CopiedMapEntries.Add(new CopiedOverlayEntry(offset, cell.Overlay.OverlayType.ININame, cell.Overlay.FrameIndex));
                    }

                    if ((EntryTypes & CopiedEntryType.Smudge) == CopiedEntryType.Smudge)
                    {
                        if (cell.Smudge != null)
                            copiedMapData.CopiedMapEntries.Add(new CopiedSmudgeEntry(offset, cell.Smudge.SmudgeType.ININame));
                    }

                    if ((EntryTypes & CopiedEntryType.TerrainObject) == CopiedEntryType.TerrainObject)
                    {
                        if (cell.TerrainObject != null)
                            copiedMapData.CopiedMapEntries.Add(new CopiedTerrainObjectEntry(offset, cell.TerrainObject.TerrainType.ININame));
                    }

                    if ((EntryTypes & CopiedEntryType.Vehicle) == CopiedEntryType.Vehicle)
                    {
                        if (cell.Vehicle != null)
                            copiedMapData.CopiedMapEntries.Add(new CopiedVehicleEntry(offset, cell.Vehicle.ObjectType.ININame, cell.Vehicle.Owner.ININame, cell.Vehicle.HP, cell.Vehicle.Veterancy, cell.Vehicle.Facing, cell.Vehicle.Mission));
                    }

                    if ((EntryTypes & CopiedEntryType.Structure) == CopiedEntryType.Structure)
                    {
                        if (cell.Structure != null && cell.Structure.Position == new Point2D(x, y))
                            copiedMapData.CopiedMapEntries.Add(new CopiedStructureEntry(offset, cell.Structure.ObjectType.ININame, cell.Structure.Owner.ININame, cell.Structure.HP, 0, cell.Structure.Facing, string.Empty));
                    }

                    if ((EntryTypes & CopiedEntryType.Infantry) == CopiedEntryType.Infantry)
                    {
                        cell.DoForAllInfantry(inf => copiedMapData.CopiedMapEntries.Add(new CopiedInfantryEntry(offset, inf.ObjectType.ININame, inf.Owner.ININame, inf.HP, inf.Veterancy, inf.Facing, inf.Mission, inf.SubCell)));
                    }
                }
            }

            System.Windows.Forms.Clipboard.SetData(Constants.ClipboardMapDataFormatValue, copiedMapData.Serialize());

            ExitAction();
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            if (StartCellCoords == null)
            {
                return;
            }

            Point2D startCellCoords = StartCellCoords.Value;
            int startY = Math.Min(cellCoords.Y, startCellCoords.Y);
            int endY = Math.Max(cellCoords.Y, startCellCoords.Y);
            int startX = Math.Min(cellCoords.X, startCellCoords.X);
            int endX = Math.Max(cellCoords.X, startCellCoords.X);

            Point2D startPoint = CellMath.CellTopLeftPointFromCellCoords_3D(new Point2D(startX, startY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX / 2, 0);
            Point2D endPoint = CellMath.CellTopLeftPointFromCellCoords_3D(new Point2D(endX, endY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX / 2, Constants.CellSizeY);
            Point2D corner1 = CellMath.CellTopLeftPointFromCellCoords_3D(new Point2D(startX, endY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(0, Constants.CellSizeY / 2);
            Point2D corner2 = CellMath.CellTopLeftPointFromCellCoords_3D(new Point2D(endX, startY), CursorActionTarget.Map) - cameraTopLeftPoint + new Point2D(Constants.CellSizeX, Constants.CellSizeY / 2);

            startPoint = startPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            endPoint = endPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            corner1 = corner1.ScaleBy(CursorActionTarget.Camera.ZoomLevel);
            corner2 = corner2.ScaleBy(CursorActionTarget.Camera.ZoomLevel);

            Color lineColor = Color.Red;
            int thickness = 2;
            Renderer.DrawLine(startPoint.ToXNAVector(), corner1.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(startPoint.ToXNAVector(), corner2.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner1.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner2.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
        }
    }
}
