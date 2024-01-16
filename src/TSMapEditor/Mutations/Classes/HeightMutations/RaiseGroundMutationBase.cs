using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models.Enums;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes.HeightMutations
{
    public abstract class RaiseGroundMutationBase : AlterElevationMutationBase
    {
        protected RaiseGroundMutationBase(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize) : base(mutationTarget)
        {
            OriginCell = originCell;
            BrushSize = brushSize;
            RampTileSet = Map.TheaterInstance.Theater.RampTileSet;
        }

        protected readonly Point2D OriginCell;
        protected readonly BrushSize BrushSize;
        protected TileSet RampTileSet;

        protected List<Point2D> cellsToProcess = new List<Point2D>();
        protected List<Point2D> processedCellsThisIteration = new List<Point2D>();
        protected List<Point2D> totalProcessedCells = new List<Point2D>();
        protected List<AlterGroundElevationUndoData> undoData = new List<AlterGroundElevationUndoData>();

        protected static readonly Point2D[] SurroundingTiles = new Point2D[] { new Point2D(-1, 0), new Point2D(1, 0), new Point2D(0, -1), new Point2D(0, 1),
                                                                             new Point2D(-1, -1), new Point2D(-1, 1), new Point2D(1, -1), new Point2D(1, 1) };

        /// <summary>
        /// Adds a cell's data to the undo data structure.
        /// Does nothing if the cell has already been added to the undo data structure.
        /// </summary>
        protected void AddCellToUndoData(Point2D cellCoords)
        {
            var cell = Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (undoData.Exists(u => u.CellCoords == cellCoords))
                return;

            undoData.Add(new AlterGroundElevationUndoData(cellCoords, cell.TileIndex, cell.SubTileIndex, cell.Level));
        }

        protected void RegisterCell(Point2D cellCoords)
        {
            if (processedCellsThisIteration.Contains(cellCoords) || cellsToProcess.Contains(cellCoords))
                return;

            cellsToProcess.Add(cellCoords);
        }

        protected void MarkCellAsProcessed(Point2D cellCoords)
        {
            processedCellsThisIteration.Add(cellCoords);
            cellsToProcess.Remove(cellCoords);

            if (!totalProcessedCells.Contains(cellCoords))
                totalProcessedCells.Add(cellCoords);
        }

        /// <summary>
        /// Entry point for raising ground.
        /// </summary>
        protected void RaiseGround()
        {
            var targetCell = Map.GetTile(OriginCell);

            if (targetCell == null || targetCell.Level >= Constants.MaxMapHeightLevel || !IsCellMorphable(targetCell))
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
                    var subTile = Map.TheaterInstance.GetTile(otherCell.TileIndex).GetSubTile(otherCell.SubTileIndex);
                    if (otherCell == null || !IsCellMorphable(otherCell) || otherCell.Level != height || subTile.TmpImage.RampType != RampType.None)
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

            int beginY = OriginCell.Y - ySize / 2;
            int endY = OriginCell.Y + ySize / 2;
            int beginX = OriginCell.X - xSize / 2;
            int endX = OriginCell.X + xSize / 2;

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

        /// <summary>
        /// Main function for ground height level alteration.
        /// Prior to this, a derived class has already raised or lowered
        /// some target cells. Now we need to figure out what kinds of changes
        /// to the map are necessary for these altered height levels to look smooth.
        /// 
        /// Algorithm goes as follows:
        /// 
        /// 1) Check surrounding cells for height differences of 2 levels, if found then
        ///    raise the relevant cells to lower the difference to only 1 level.
        ///    Repeat this process recursively until there are no cells to change.
        /// 
        /// 2) Apply some miscellaneous cell height fixes, the game does not have ramps for
        ///    every potential case.
        /// 
        /// 3) Check the affected cells and their neighbours for necessary ramp changes.
        /// </summary>
        protected void Process()
        {
            ProcessCells();

            CellHeightFixes();

            ApplyRamps();

            MutationTarget.InvalidateMap();
        }

        private void ProcessCells()
        {
            while (cellsToProcess.Count > 0)
            {
                var cellsCopy = new List<Point2D>(cellsToProcess);
                cellsToProcess.Clear();
                processedCellsThisIteration.Clear();

                foreach (var cell in cellsCopy)
                    CheckCell(cell);
            }
        }

        protected abstract void CheckCell(Point2D cellCoords);

        protected abstract TransitionRampInfo[] GetHeightFixers();

        /// <summary>
        /// Applies miscellaneous fixes to height data of processed cells.
        /// </summary>
        private void CellHeightFixes()
        {
            // We process one set of cells at a time, starting from the cells
            // processed so far.
            // During the processing, we might get new cells to process.
            // We repeat the process until no new cells to process have been added to the list.
            List<Point2D> cellsToCheck = new(totalProcessedCells);
            List<Point2D> newCells = new();

            while (cellsToCheck.Count > 0)
            {
                // If a cell has higher cells on both of its sides in a straight diagonal line,
                // we need to normalize its height to be on the same level with the diagonal sides,
                // because no "one cell" depression exists
                cellsToCheck.ForEach(cc => CellHeightFix_CheckStraightDiagonalLine(cc, newCells, true));
                cellsToCheck.ForEach(cc => CellHeightFix_CheckStraightDiagonalLine(cc, newCells, false));

                // If a cell has a two-levels-higher cell next to it on a horizontal or vertical line,
                // and a one-level-higher cell next to it on the opposite side,
                // we need to increase its level by 1
                totalProcessedCells.ForEach(cc => CellHeightFix_DifferentHeightDiffsOnStraightLine(cc, new Point2D(1, -1), new Point2D(-1, 1)));
                totalProcessedCells.ForEach(cc => CellHeightFix_DifferentHeightDiffsOnStraightLine(cc, new Point2D(-1, -1), new Point2D(1, 1)));

                // Special height fixes
                totalProcessedCells.ForEach(cc =>
                {
                    var cell = Map.GetTile(cc);
                    if (cell == null)
                        return;

                    if (!IsCellMorphable(cell))
                        return;

                    var applyingTransition = Array.Find(GetHeightFixers(), tri => tri.Matches(Map, cc, cell.Level));
                    if (applyingTransition != null)
                    {
                        AddCellToUndoData(cell.CoordsToPoint());
                        cell.ChangeTileIndex(0, 0);
                        cell.Level += (byte)applyingTransition.HeightChange;

                        // If we fixed this flaw from one cell, also check the surrounding ones
                        foreach (var surroundingCell in SurroundingTiles)
                        {
                            if (!newCells.Contains(cc + surroundingCell))
                                newCells.Add(cc + surroundingCell);
                        }
                    }
                });

                var cellsForNextRound = newCells; //.Where(c => !cellsToCheck.Contains(c));
                cellsToCheck.Clear();
                cellsToCheck.AddRange(cellsForNextRound);
                totalProcessedCells.AddRange(cellsForNextRound.Where(c => !totalProcessedCells.Contains(c)));
                newCells.Clear();
            }
        }

        private void CellHeightFix_CheckStraightDiagonalLine(Point2D cellCoords, List<Point2D> newCells, bool isXAxis)
        {
            var cell = Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (!IsCellMorphable(cell))
                return;

            Point2D otherCellCoords = cellCoords + new Point2D(isXAxis ? 1 : 0, isXAxis ? 0 : 1);
            var otherCell = Map.GetTile(otherCellCoords);
            if (otherCell == null || otherCell.Level <= cell.Level)
                return;

            int otherLevel = otherCell.Level;

            otherCellCoords = cellCoords + new Point2D(isXAxis ? -1 : 0, isXAxis ? 0 : -1);
            otherCell = Map.GetTile(otherCellCoords);
            if (otherCell == null)
                return;

            if (otherCell.Level == otherLevel)
            {
                AddCellToUndoData(cell.CoordsToPoint());
                cell.ChangeTileIndex(0, 0);
                cell.Level = otherCell.Level;

                // If we fixed this flaw from one cell, also check the surrounding ones
                foreach (var surroundingCell in SurroundingTiles)
                {
                    if (!newCells.Contains(cellCoords + surroundingCell))
                        newCells.Add(cellCoords + surroundingCell);
                }
            }
        }

        private void CellHeightFix_DifferentHeightDiffsOnStraightLine(Point2D cellCoords, Point2D offset1, Point2D offset2)
        {
            var cell = Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (!IsCellMorphable(cell))
                return;

            int totalLevelDifference = 0;

            Point2D otherCellCoords = cellCoords + offset1;
            var otherCell = Map.GetTile(otherCellCoords);
            if (otherCell != null)
                totalLevelDifference += otherCell.Level - cell.Level;

            otherCellCoords = cellCoords + offset2;
            otherCell = Map.GetTile(otherCellCoords);
            if (otherCell != null)
                totalLevelDifference += otherCell.Level - cell.Level;

            if (totalLevelDifference >= 3)
            {
                AddCellToUndoData(cell.CoordsToPoint());
                cell.ChangeTileIndex(0, 0);
                cell.Level++;
            }
        }

        protected abstract TransitionRampInfo[] GetTransitionRampInfos();

        private void ApplyRamps()
        {
            var cellsToProcess = new List<Point2D>(totalProcessedCells);

            // Go through all processed cells and add their neighbours to be processed too
            totalProcessedCells.ForEach(cc =>
            {
                for (int i = 0; i < (int)Direction.Count; i++)
                {
                    var otherCellCoords = cc + Helpers.VisualDirectionToPoint((Direction)i);

                    if (!cellsToProcess.Contains(otherCellCoords))
                        cellsToProcess.Add(otherCellCoords);
                }
            });

            foreach (var cellCoords in cellsToProcess)
            {
                var cell = Map.GetTile(cellCoords);
                if (cell == null)
                    continue;

                if (!IsCellMorphable(cell))
                    continue;

                LandType landType = (LandType)Map.TheaterInstance.GetTile(cell.TileIndex).GetSubTile(cell.SubTileIndex).TmpImage.TerrainType;
                if (landType == LandType.Rock || landType == LandType.Water)
                    continue;

                foreach (var transitionRampInfo in GetTransitionRampInfos())
                {
                    if (transitionRampInfo.Matches(Map, cellCoords, cell.Level))
                    {
                        AddCellToUndoData(cell.CoordsToPoint());

                        if (transitionRampInfo.RampType == RampType.None)
                        {
                            cell.ChangeTileIndex(0, 0);
                        }
                        else
                        {
                            cell.ChangeTileIndex(RampTileSet.StartTileIndex + ((int)transitionRampInfo.RampType - 1), 0);
                        }

                        if (transitionRampInfo.HeightChange != 0)
                        {
                            cell.Level += (byte)transitionRampInfo.HeightChange;
                        }

                        break;
                    }
                }
            }
        }

        public override void Undo()
        {
            foreach (var entry in undoData)
            {
                var cell = Map.GetTile(entry.CellCoords);
                cell.ChangeTileIndex(entry.TileIndex, (byte)entry.SubTileIndex);
                cell.Level = (byte)entry.HeightLevel;
            }

            MutationTarget.InvalidateMap();
        }
    }
}
