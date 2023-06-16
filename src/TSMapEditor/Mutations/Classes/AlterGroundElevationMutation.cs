using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    using HCT = HeightComparisonType;

    /// <summary>
    /// Defines what kind of comparison to use when comparing the height of a cell
    /// to the height of another cell.
    /// </summary>
    enum HeightComparisonType
    {
        /// <summary>
        /// The height of the other cell is irrelevant for the resulting ramp type.
        /// </summary>
        Irrelevant,

        /// <summary>
        /// The other cell must be higher by 1 level.
        /// </summary>
        Higher,

        /// <summary>
        /// The other cell must be higher by 2 or more levels.
        /// </summary>
        MuchHigher,

        /// <summary>
        /// The other cell must be higher (by 1 level) or equal.
        /// </summary>
        HigherOrEqual,

        /// <summary>
        /// The other cell must be lower by 1 level.
        /// </summary>
        Lower,

        /// <summary>
        /// The other cell must be lower by 2 or more levels.
        /// </summary>
        MuchLower,

        /// <summary>
        /// The other cell must be lower or equal.
        /// </summary>
        LowerOrEqual,

        /// <summary>
        /// The other cell must be equal.
        /// </summary>
        Equal
    }

    public abstract class AlterGroundElevationMutation : Mutation
    {
        /// <summary>
        /// Struct for the undo data of mutations based on this class.
        /// </summary>
        struct AlterGroundElevationUndoData
        {
            public Point2D CellCoords;
            public int TileIndex;
            public int SubTileIndex;
            public int HeightLevel;

            public AlterGroundElevationUndoData(Point2D cellCoords, int tileIndex, int subTileIndex, int heightLevel)
            {
                CellCoords = cellCoords;
                TileIndex = tileIndex;
                SubTileIndex = subTileIndex;
                HeightLevel = heightLevel;
            }
        }

        /// <summary>
        /// Defines a condition for applying a ramp on a cell based on the 
        /// height level difference between it and its neighbouring cells.
        /// </summary>
        class TransitionRampInfo
        {
            public TransitionRampInfo(RampType rampType, List<HCT> comparisonTypesForDirections, int heightChange = 0)
            {
                RampType = rampType;
                ComparisonTypesForDirections = comparisonTypesForDirections;
                HeightChange = heightChange;

                if (comparisonTypesForDirections.Count != (int)Direction.Count)
                {
                    throw new ArgumentException($"The length of {nameof(comparisonTypesForDirections)} must match " +
                        $"the number of primary directions in the game ({Direction.Count})");
                }
            }

            public readonly RampType RampType;

            public readonly List<HCT> ComparisonTypesForDirections;

            public int HeightChange { get; }

            public bool Matches(Map map, Point2D cellCoords, int cellHeight)
            {
                for (int i = 0; i < (int)Direction.Count; i++)
                {
                    var offset = Helpers.VisualDirectionToPoint((Direction)i);

                    var otherCellCoords = cellCoords + offset;
                    var otherCell = map.GetTile(otherCellCoords);
                    if (otherCell == null)
                        continue;

                    HCT expected = ComparisonTypesForDirections[i];

                    switch (expected)
                    {
                        case HCT.Irrelevant:
                            continue;

                        case HCT.Higher:
                            if (otherCell.Level != cellHeight + 1)
                                return false;
                            break;

                        case HCT.MuchHigher:
                            if (otherCell.Level - cellHeight < 2)
                                return false;
                            break;

                        case HCT.HigherOrEqual:
                            if (otherCell.Level < cellHeight /*|| otherCell.Level - cellHeight > 1*/)
                                return false;
                            break;

                        case HCT.Lower:
                            if (otherCell.Level != cellHeight - 1) 
                                return false;
                            break;

                        case HCT.MuchLower:
                            if (cellHeight - otherCell.Level < 2)
                                return false;
                            break;

                        case HCT.LowerOrEqual:
                            if (otherCell.Level > cellHeight /*|| cellHeight - otherCell.Level > 1*/)
                                return false;
                            break;

                        case HCT.Equal:
                            if (otherCell.Level != cellHeight)
                                return false;
                            break;
                    }
                }

                return true;
            }
        }

        public AlterGroundElevationMutation(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize) : base(mutationTarget)
        {
            this.OriginCell = originCell;
            this.BrushSize = brushSize;
            RampTileSet = Map.TheaterInstance.Theater.TileSets.Find(ts => ts.SetName == "Ramps");
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

            new TransitionRampInfo(RampType.SteepSE, new() { HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.MuchHigher, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual }),
            new TransitionRampInfo(RampType.SteepSW, new() { HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.MuchHigher, HCT.Higher, HCT.HigherOrEqual }),
            new TransitionRampInfo(RampType.SteepNW, new() { HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual, HCT.Higher, HCT.MuchHigher }),
            new TransitionRampInfo(RampType.SteepNE, new() { HCT.Higher, HCT.MuchHigher, HCT.Higher, HCT.HigherOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.LowerOrEqual, HCT.HigherOrEqual }),

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

            // Special on-ramp-placement height fix checks
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher }, 1),
            new TransitionRampInfo(RampType.None, new() { HCT.Equal, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal }, 1),

            // In case it's anything else, we probably need to flatten it
            new TransitionRampInfo(RampType.None, new() { HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant, HCT.Irrelevant }, 0),
        };

        // Pre-ramp-placement height fix checks
        private static readonly TransitionRampInfo[] heightFixers = new[]
        {
            new TransitionRampInfo(RampType.None, new() { HCT.Higher, HCT.Higher, HCT.Higher, HCT.Higher, HCT.Equal, HCT.Equal, HCT.Equal, HCT.Equal }, 1),
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

            if (!thisCell.IsClearGround() && !RampTileSet.ContainsTile(thisCell.TileIndex))
                return;

            int biggestHeightDiff = 0;

            var northernCell = Map.GetTile(cellCoords + new Point2D(0, -1));
            if (northernCell != null && northernCell.Level > thisCell.Level)
            {
                biggestHeightDiff = Math.Max(biggestHeightDiff, northernCell.Level - thisCell.Level);
            }

            var southernCell = Map.GetTile(cellCoords + new Point2D(0, 1));
            if (southernCell != null && southernCell.Level > thisCell.Level)
            {
                biggestHeightDiff = Math.Max(biggestHeightDiff, southernCell.Level - thisCell.Level);
            }

            var westernCell = Map.GetTile(cellCoords + new Point2D(-1, 0));
            if (westernCell != null && westernCell.Level > thisCell.Level)
            {
                biggestHeightDiff = Math.Max(biggestHeightDiff, westernCell.Level - thisCell.Level);
            }
            
            var easternCell = Map.GetTile(cellCoords + new Point2D(1, 0));
            if (easternCell != null && easternCell.Level > thisCell.Level)
            {
                biggestHeightDiff = Math.Max(biggestHeightDiff, easternCell.Level - thisCell.Level);
            }

            // If nearby cells are raised by more than 1 cell, it's necessary to also raise this cell
            if (biggestHeightDiff > 1)
            {
                AddCellToUndoData(thisCell.CoordsToPoint());
                thisCell.Level += (byte)(biggestHeightDiff - 1);

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

            if (!cell.IsClearGround() && !RampTileSet.ContainsTile(cell.TileIndex))
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

            if (!cell.IsClearGround() && !RampTileSet.ContainsTile(cell.TileIndex))
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

                    if (!cell.IsClearGround() && !RampTileSet.ContainsTile(cell.TileIndex))
                        return;

                    var applyingTransition = Array.Find(heightFixers, tri => tri.Matches(Map, cc, cell.Level));
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

                if (!cell.IsClearGround() && !RampTileSet.ContainsTile(cell.TileIndex))
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
