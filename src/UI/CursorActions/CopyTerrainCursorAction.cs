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

        public Point2D? StartCellCoords { get; set; } = null;

        public override void LeftClick(Point2D cellCoords)
        {
            if (StartCellCoords == null)
            {
                StartCellCoords = cellCoords;
                return;
            }

            CursorActionTarget.CopiedTerrainData.Clear();

            Point2D startCellCoords = StartCellCoords.Value;
            int startY = Math.Min(cellCoords.Y, startCellCoords.Y);
            int endY = Math.Max(cellCoords.Y, startCellCoords.Y);
            int startX = Math.Min(cellCoords.X, startCellCoords.X);
            int endX = Math.Max(cellCoords.X, startCellCoords.X);

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    MapTile cell = CursorActionTarget.Map.GetTile(x, y);
                    if (cell != null)
                    {
                        CursorActionTarget.CopiedTerrainData.Add(
                            new CopiedTerrainData(cell.TileIndex, cell.SubTileIndex, new Point2D(x - startX, y - startY)));
                    }
                }
            }

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

            Point2D startPoint = CellMath.CellTopLeftPoint(new Point2D(startX, startY), CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;
            Point2D endPoint = CellMath.CellTopLeftPoint(new Point2D(endX, endY), CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;
            Point2D corner1 = CellMath.CellTopLeftPoint(new Point2D(startX, endY), CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;
            Point2D corner2 = CellMath.CellTopLeftPoint(new Point2D(endX, startY), CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;

            Color lineColor = Color.Red;
            int thickness = 2;
            Renderer.DrawLine(startPoint.ToXNAVector(), corner1.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(startPoint.ToXNAVector(), corner2.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner1.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
            Renderer.DrawLine(corner2.ToXNAVector(), endPoint.ToXNAVector(), lineColor, thickness);
        }
    }
}
