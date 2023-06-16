using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation for "smartly" raising ground, with automatic application of ramps.
    /// </summary>
    internal class RaiseGroundMutation : AlterGroundElevationMutation
    {
        public RaiseGroundMutation(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize) : base(mutationTarget, originCell, brushSize)
        {
        }

        public override void Perform() => RaiseGround();


        private void RaiseGround()
        {
            var targetCell = Map.GetTile(OriginCell);

            if (targetCell == null || targetCell.Level >= Constants.MaxMapHeightLevel || (!targetCell.IsClearGround() && !RampTileSet.ContainsTile(targetCell.TileIndex)))
                return;

            int targetCellHeight = targetCell.Level;

            int xSize = BrushSize.Width - 2;
            int ySize = BrushSize.Height - 2;

            // Special case for 2x2 brush.
            // Check if we can create a 2x2 "hill". If yes, then do so.
            // Otherwise, process it as 1x1.
            if (BrushSize.Width == 2 && BrushSize.Height == 2)
            {
                bool canCreateSmallHill = true;
                int height = targetCell.Level;
                BrushSize.DoForBrushSize(offset =>
                {
                    var otherCellCoords = OriginCell + offset;
                    var otherCell = Map.GetTile(otherCellCoords);
                    if (otherCell == null || !otherCell.IsClearGround() || otherCell.Level != height)
                        canCreateSmallHill = false;
                });

                if (canCreateSmallHill)
                {
                    CreateSmallHill(OriginCell);
                    return;
                }
            }

            // If the brush size is 1, only process it if the target cell is a ramp.
            // If it is not a ramp, then we'd need to raise the cell's height,
            // which would always result in it affecting more than 1 cell,
            // which wouldn't be logical with the brush size.
            if (BrushSize.Width == 1 || BrushSize.Height == 1)
            {
                if (!RampTileSet.ContainsTile(targetCell.TileIndex))
                    return;
            }

            int beginY = OriginCell.Y - (ySize / 2);
            int endY = OriginCell.Y + (ySize / 2);
            int beginX = OriginCell.X - (xSize / 2);
            int endX = OriginCell.X + (xSize / 2);

            // For all other brush sizes we can have a generic implementation
            for (int y = beginY; y <= endY; y++)
            {
                for (int x = beginX; x <= endX; x++)
                {
                    var cellCoords = new Point2D(x, y);
                    targetCell = Map.GetTile(cellCoords);
                    if (targetCell == null || targetCell.Level >= Constants.MaxMapHeightLevel)
                        continue;

                    // Only raise ground that was on the same level with our original target cell,
                    // otherwise things get illogical
                    if (targetCell.Level != targetCellHeight)
                        continue;

                    // Raise this cell and check surrounding cells whether they need ramps
                    AddCellToUndoData(cellCoords);
                    targetCell.Level++;
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

        private void CreateSmallHill(Point2D cellCoords)
        {
            AddCellToUndoData(cellCoords);
            AddCellToUndoData(cellCoords + new Point2D(1, 0));
            AddCellToUndoData(cellCoords + new Point2D(0, 1));
            AddCellToUndoData(cellCoords + new Point2D(1, 1));

            Map.GetTile(cellCoords).ChangeTileIndex(RampTileSet.StartTileIndex + ((int)RampType.CornerNW - 1), 0);
            Map.GetTile(cellCoords + new Point2D(1, 0)).ChangeTileIndex(RampTileSet.StartTileIndex + ((int)RampType.CornerNE - 1), 0);
            Map.GetTile(cellCoords + new Point2D(0, 1)).ChangeTileIndex(RampTileSet.StartTileIndex + ((int)RampType.CornerSW - 1), 0);
            Map.GetTile(cellCoords + new Point2D(1, 1)).ChangeTileIndex(RampTileSet.StartTileIndex + ((int)RampType.CornerSE - 1), 0);

            MutationTarget.InvalidateMap();
        }
    }
}
