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

        public static Point2D CellCoordsFromPixelCoords(Point2D pixelCoords, Point2D mapSize)
        {
            // It's likely possible to find a more efficient algorithm for this

            int cx = 1;
            int cy = mapSize.X;

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
            else if (rx + ry > Constants.CellSizeX)
                cx++;
            else if (rx > Constants.CellSizeY + ry * 2)
                cy--;
            else if (rx < (ry - Constants.CellSizeY / 2) * 2)
                cy++;

            return new Point2D(cx, cy);
        }
    }
}
