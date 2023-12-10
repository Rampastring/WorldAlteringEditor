using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models.Enums;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI;
using HCT = TSMapEditor.Mutations.Classes.HeightMutations.HeightComparisonType;
using SharpDX.Direct3D9;

namespace TSMapEditor.Mutations.Classes.HeightMutations
{
    internal class FSLowerGroundMutation : AlterElevationMutationBase
    {
        public FSLowerGroundMutation(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize) : base(mutationTarget)
        {
            OriginCell = originCell;
            BrushSize = brushSize;
            RampTileSet = Map.TheaterInstance.Theater.RampTileSet;
        }

        public override void Perform() => LowerGround();

        private void LowerGround()
        {
            var targetCell = Map.GetTile(OriginCell);

            if (targetCell == null || targetCell.Level < 1 || !IsCellMorphable(targetCell))
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

            int beginY = OriginCell.Y - (ySize - 1) / 2;
            int endY = OriginCell.Y + ySize / 2;
            int beginX = OriginCell.X - (xSize - 1) / 2;
            int endX = OriginCell.X + xSize / 2;

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

        protected readonly Point2D OriginCell;
        protected readonly BrushSize BrushSize;
        protected TileSet RampTileSet;

        private List<Point2D> cellsToProcess = new List<Point2D>();
        private List<Point2D> processedCellsThisIteration = new List<Point2D>();
        private List<Point2D> totalProcessedCells = new List<Point2D>();
        private List<AlterGroundElevationUndoData> undoData = new List<AlterGroundElevationUndoData>();

        private static readonly TransitionRampInfo[] transitionRampInfos = new[]
        {
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal }),

            new TransitionRampInfo(RampType.West, new() { HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual } ),
            new TransitionRampInfo(RampType.North, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.East, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.South, new() { HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual }),

            new TransitionRampInfo(RampType.CornerNW, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.Equal, HCT.Higher, HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.CornerNE, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.Equal, HCT.Higher, HCT.Equal, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.CornerSE, new() { HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.Equal, HCT.Higher }),
            new TransitionRampInfo(RampType.CornerSW, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),

            new TransitionRampInfo(RampType.MidNW, new() { HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Equal, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.MidNE, new() { HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidSE, new() { HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher }),
            new TransitionRampInfo(RampType.MidSW, new() { HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Equal, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual }),

            new TransitionRampInfo(RampType.DoubleUpSWNE, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.Irrelevant, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Irrelevant }),
            new TransitionRampInfo(RampType.DoubleDownSWNE, new() { HCT.Equal, HCT.Irrelevant, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Irrelevant, HCT.Equal, HCT.Higher }),

            // Fixes for ramps in "odd angles" between cells

            new TransitionRampInfo(RampType.MidNE, new() { HCT.Equal, HCT.LowerOrEqual, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant }),
            new TransitionRampInfo(RampType.MidSW, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Irrelevant, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Higher }),

            new TransitionRampInfo(RampType.MidNW, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.MidSE, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.LowerOrEqual, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal }),

            new TransitionRampInfo(RampType.MidSE, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.LowerOrEqual, HCT.Equal, HCT.Irrelevant, HCT.Higher, HCT.Equal }),
            new TransitionRampInfo(RampType.MidNW, new() { HCT.Equal, HCT.Irrelevant, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.LowerOrEqual }),

            new TransitionRampInfo(RampType.MidNE, new() { HCT.Equal, HCT.LowerOrEqual, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }),
            new TransitionRampInfo(RampType.MidSW, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.LowerOrEqual, HCT.Equal }),

            // "Less likely" cases of mid-ramps, where the cell direclty behind the ramp is not higher but the cells on the "backsides" are
            new TransitionRampInfo(RampType.MidNW, new() { HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.MidNW, new() { HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.MidNE, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidNE, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidSE, new() { HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidSE, new() { HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidSW, new() { HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.MidSW, new() { HCT.Higher, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual }),

            // In case it's anything else, we probably need to flatten it
            new TransitionRampInfo(RampType.None, new() { HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant }, 0),
        };

        // Pre-ramp-placement height fix checks
        private static readonly TransitionRampInfo[] heightFixers = new[]
        {
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.HigherOrEqual, HCT.Higher, HCT.Equal, HCT.Higher, HCT.Equal, HCT.HigherOrEqual, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.Higher, HCT.Equal, HCT.HigherOrEqual, HCT.Higher, HCT.Equal, HCT.HigherOrEqual }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.HigherOrEqual, HCT.Higher, HCT.Equal, HCT.HigherOrEqual, HCT.Higher, HCT.Equal, HCT.Higher, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.HigherOrEqual, HCT.Higher, HCT.Equal, HCT.HigherOrEqual, HCT.Higher, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Irrelevant, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant, HCT.Higher }, 1),

            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Irrelevant, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant, HCT.Higher, HCT.Irrelevant }, 1),
        };

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

        private void ProcessCells()
        {
            while (cellsToProcess.Count > 0)
            {
                var cellsCopy = new List<Point2D>(cellsToProcess);
                cellsToProcess.Clear();
                processedCellsThisIteration.Clear();

                foreach (var cell in cellsCopy)
                    RecursiveCheckCell(cell);
            }
        }

        private void RecursiveCheckCell(Point2D cellCoords)
        {
            if (processedCellsThisIteration.Contains(cellCoords) || cellsToProcess.Contains(cellCoords))
                return;

            MarkCellAsProcessed(cellCoords);

            var thisCell = Map.GetTile(cellCoords);
            if (thisCell == null)
                return;

            if (!IsCellMorphable(thisCell))
                return;

            int biggestHeightDiff = 0;

            for (int direction = 0; direction < (int)Direction.Count; direction++)
            {
                var cell = Map.GetTile(cellCoords + Helpers.VisualDirectionToPoint((Direction)direction));
                if (cell == null)
                    continue;

                if (!IsCellMorphable(cell))
                    continue;

                if (cell.Level < thisCell.Level)
                    biggestHeightDiff = Math.Max(biggestHeightDiff, thisCell.Level - cell.Level);
            }

            // If nearby cells are lower by more than 1 cell, it's necessary to also lower this cell
            if (biggestHeightDiff > 1)
            {
                AddCellToUndoData(thisCell.CoordsToPoint());

                if (thisCell.Level > 0)
                {
                    if (thisCell.Level >= biggestHeightDiff - 1)
                        thisCell.Level = (byte)(thisCell.Level - (biggestHeightDiff - 1));
                    else
                        thisCell.Level--;
                }

                foreach (Point2D offset in SurroundingTiles)
                {
                    RegisterCell(cellCoords + offset);
                }
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
                // cellsToCheck.ForEach(cc => CellHeightFix_CheckStraightDiagonalLine(cc, newCells, true));
                // cellsToCheck.ForEach(cc => CellHeightFix_CheckStraightDiagonalLine(cc, newCells, false));

                // If a cell has a two-levels-higher cell next to it on a horizontal or vertical line,
                // and a one-level-higher cell next to it on the opposite side,
                // we need to increase its level by 1
                // totalProcessedCells.ForEach(cc => CellHeightFix_DifferentHeightDiffsOnStraightLine(cc, new Point2D(1, -1), new Point2D(-1, 1)));
                // totalProcessedCells.ForEach(cc => CellHeightFix_DifferentHeightDiffsOnStraightLine(cc, new Point2D(-1, -1), new Point2D(1, 1)));

                // Special height fixes
                totalProcessedCells.ForEach(cc =>
                {
                    var cell = Map.GetTile(cc);
                    if (cell == null)
                        return;

                    if (!IsCellMorphable(cell))
                        return;

                    var applyingTransition = Array.Find(heightFixers, tri => tri.Matches(Map, cc, cell.Level));
                    if (applyingTransition != null)
                    {
                        AddCellToUndoData(cell.CoordsToPoint());
                        cell.ChangeTileIndex(0, 0);
                        cell.Level = (byte)(cell.Level + applyingTransition.HeightChange);

                        // If we fixed this flaw from one cell, also check the surrounding ones
                        foreach (var surroundingCell in SurroundingTiles)
                        {
                            if (!newCells.Contains(cc + surroundingCell))
                                newCells.Add(cc + surroundingCell);
                        }
                    }
                });

                var cellsForNextRound = newCells.Where(c => !cellsToCheck.Contains(c));
                cellsToCheck.Clear();
                cellsToCheck.AddRange(cellsForNextRound);
                totalProcessedCells.AddRange(cellsForNextRound.Where(c => !totalProcessedCells.Contains(c)));
                newCells.Clear();
            }
        }

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

                foreach (var transitionRampInfo in transitionRampInfos)
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
                            cell.Level = (byte)(cell.Level + transitionRampInfo.HeightChange);
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
