using Microsoft.Xna.Framework;
using TSMapEditor.Models;

namespace TSMapEditor.GameMath
{
    /// <summary>
    /// Provides static methods for calculating math related to cells.
    /// </summary>
    public static class CellMath
    {
        public static Point2D CellTopLeftPointFromCellCoords(Point2D cellCoords, Map map)
        {
            int cx = (cellCoords.X - 1) * (Constants.CellSizeX / 2);
            int cy = (cellCoords.X - 1) * (Constants.CellSizeY / 2);

            int diff = map.Size.X - cellCoords.Y;
            cx += diff * (Constants.CellSizeX / 2);
            cy -= diff * (Constants.CellSizeY / 2);
            return new Point2D(cx, cy);
        }

        public static Point2D CellTopLeftPointFromCellCoords_3D(Point2D cellCoords, Map map)
        {
            var cell = map.GetTile(cellCoords);

            if (cell == null)
                return CellTopLeftPointFromCellCoords(cellCoords, map);

            Point2D preHeightCoords = CellTopLeftPointFromCellCoords(cellCoords, map);
            return preHeightCoords - new Point2D(0, cell.Level * Constants.CellHeight);
        }

        public static Point2D CellCenterPointFromCellCoords(Point2D cellCoords, Map map)
            => CellTopLeftPointFromCellCoords(cellCoords, map) + new Point2D(Constants.CellSizeX / 2, Constants.CellSizeY / 2);

        public static Point2D CellCenterPointFromCellCoords_3D(Point2D cellCoords, Map map)
        {
            var cell = map.GetTile(cellCoords);

            if (cell == null)
                return CellCenterPointFromCellCoords(cellCoords, map);

            Point2D preHeightCoords = CellCenterPointFromCellCoords(cellCoords, map);
            return preHeightCoords - new Point2D(0, cell.Level * Constants.CellHeight);
        }

        public static Point2D CellCoordsFromPixelCoords_2D(Point2D pixelCoords, Map map)
        {
            // It's likely possible to find a more efficient algorithm for this

            // Find initial cell
            int cx = 1;
            int cy = map.Size.X;

            int xm = pixelCoords.X / Constants.CellSizeX;
            cx += xm;
            cy -= xm;

            int ym = pixelCoords.Y / Constants.CellSizeY;
            cx += ym;
            cy += ym;

            int rx = pixelCoords.X % Constants.CellSizeX;
            int ry = pixelCoords.Y % Constants.CellSizeY;

            if (rx + ry * 2 < Constants.CellSizeY)
                cx--;
            else if (rx + 2 * (ry - (Constants.CellSizeY / 2)) > Constants.CellSizeX)
                cx++;
            else if (rx > Constants.CellSizeY + ry * 2)
                cy--;
            else if (rx < (ry - Constants.CellSizeY / 2) * 2)
                cy++;

            return new Point2D(cx, cy);
        }

        public static Point2D CellCoordsFromPixelCoords(Point2D pixelCoords, Map map)
        {
            Point2D coords2D = CellCoordsFromPixelCoords_2D(pixelCoords, map);

            if (Constants.IsFlatWorld)
                return coords2D;

            Point2D nearestCenterCoords = new Point2D(-1, -1);
            float nearestDistance = float.MaxValue;

            // Scan all cells that are deemed to be "close enough" to the initial cell
            // to see if our cursor is on the cell
            for (int y = -3; y <= 3; y++)
            {
                for (int x = -3; x <= 3; x++)
                {
                    // traverse height
                    for (int i = 0; i < 14; i++)
                    {
                        var otherCellCoords = coords2D + new Point2D(x, y) + new Point2D(i, i);
                        var otherCell = map.GetTile(otherCellCoords);

                        if (otherCell != null)
                        {
                            var tlCoords = CellTopLeftPointFromCellCoords(otherCellCoords, map);
                            var centerCoords = CellCenterPointFromCellCoords_3D(otherCellCoords, map);

                            centerCoords = new Point2D(centerCoords.X / 2, centerCoords.Y);
                            var twpx = new Point2D(pixelCoords.X / 2, pixelCoords.Y);

                            float distance = Vector2.Distance(centerCoords.ToXNAVector(), twpx.ToXNAVector());
                            if (distance <= nearestDistance)
                            {
                                nearestDistance = distance;
                                nearestCenterCoords = otherCellCoords;
                            }
                        }
                    }
                }
            }

            return nearestCenterCoords;
        }
    }
}
