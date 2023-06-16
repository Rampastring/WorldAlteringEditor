using TSMapEditor.GameMath;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    internal class LowerGroundMutation : AlterGroundElevationMutation
    {
        public LowerGroundMutation(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize) : base(mutationTarget, originCell, brushSize)
        {
        }

        public override void Perform() => LowerGround();

        private void LowerGround()
        {
            var targetCell = Map.GetTile(OriginCell);

            if (targetCell == null || targetCell.Level < 1 || (!targetCell.IsClearGround() && !RampTileSet.ContainsTile(targetCell.TileIndex)))
                return;

            int targetCellHeight = targetCell.Level;

            // If the brush size is 1, only process it if the target cell is a ramp.
            // If it is not a ramp, then we'd need to lower the cell's height,
            // which would always result in it affecting more than 1 cell,
            // which wouldn't be logical with the brush size.
            if (BrushSize.Width == 1 || BrushSize.Height == 1)
            {
                if (!RampTileSet.ContainsTile(targetCell.TileIndex))
                    return;
            }

            int xSize = BrushSize.Width;
            int ySize = BrushSize.Height;

            int beginY = OriginCell.Y - ((ySize - 1) / 2);
            int endY = OriginCell.Y + (ySize / 2);
            int beginX = OriginCell.X - ((xSize - 1) / 2);
            int endX = OriginCell.X + (xSize / 2);

            // For all other brush sizes we can have a generic implementation
            for (int y = beginY; y <= endY; y++)
            {
                for (int x = beginX; x <= endX; x++)
                {
                    var cellCoords = new Point2D(x, y);
                    targetCell = Map.GetTile(cellCoords);
                    if (targetCell == null || targetCell.Level < 1)
                        continue;

                    // Only lower ground that was on the same level with our original target cell,
                    // otherwise things get illogical
                    if (targetCell.Level != targetCellHeight)
                        continue;

                    // Lower this cell and check surrounding cells whether they need ramps
                    AddCellToUndoData(cellCoords);
                    targetCell.Level--;
                    targetCell.ChangeTileIndex(0, 0);
                    foreach (Point2D surroundingCellOffset in SurroundingTiles)
                    {
                        RegisterCell(cellCoords + surroundingCellOffset);
                    }

                    MarkCellAsProcessed(cellCoords);
                }
            }

            Process();
        }
    }
}
