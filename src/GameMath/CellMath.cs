using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.GameMath
{
    /// <summary>
    /// Provides static methods for calculating math related to cells.
    /// </summary>
    public static class CellMath
    {
        public static Point2D CellTopLeftPoint(Point2D cellCoords, int mapWidth)
        {
            int cx = cellCoords.X * (Constants.CellSizeX / 2);
            int cy = cellCoords.X * (Constants.CellSizeY / 2);

            int diff = mapWidth - cellCoords.Y;
            cx += diff * (Constants.CellSizeX / 2);
            cy -= diff * (Constants.CellSizeY / 2);
            return new Point2D(cx, cy);
        }
    }
}
