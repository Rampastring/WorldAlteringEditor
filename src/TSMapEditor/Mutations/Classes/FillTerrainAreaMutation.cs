using System;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that fills an area with terrain.
    /// </summary>
    public partial class FillTerrainAreaMutation : Mutation
    {
        public FillTerrainAreaMutation(IMutationTarget mutationTarget, MapTile target, TileImage tile) : base(mutationTarget)
        {
            if (tile.Width > 1 || tile.Height > 1)
                throw new InvalidOperationException("Only 1x1 tiles can be used to fill areas.");

            targetTile = target;
            this.tile = tile;
        }

        private MapTile targetTile;
        private TileImage tile;
        private OriginalCellTerrainData[] undoData;

        public override void Perform()
        {
            TileImage tileGraphics = MutationTarget.TheaterGraphics.GetTileGraphics(targetTile.TileIndex);
            MGTMPImage subCellImage = tileGraphics.TMPImages[targetTile.SubTileIndex];

            var originalData = new List<OriginalCellTerrainData>();

            byte terrainType = subCellImage.TmpImage.TerrainType;
            int tileSetId = tileGraphics.TileSetId;
            var tilesToCheck = new LinkedList<Point2D>(); // list of pending tiles to check
            var tileCheckHashSet = new HashSet<int>();    // hash set of tiles that have been added to the list of
                                                          // tiles to check at some point and so should not be added there again
            tilesToCheck.AddFirst(targetTile.CoordsToPoint());
            tileCheckHashSet.Add(targetTile.CoordsToPoint().GetHashCode());

            var tilesToSkip = new HashSet<int>();         // tiles that have been confirmed as not being part of the area to fill
            var tilesToProcess = new List<Point2D>();     // tiles that have been confirmed as being part of the area to fill

            while (tilesToCheck.First != null)
            {
                var coords = tilesToCheck.First.Value;
                tilesToCheck.RemoveFirst();

                if (tilesToSkip.Contains(coords.GetHashCode()))
                    continue;

                var cell = MutationTarget.Map.GetTile(coords);
                if (cell == null)
                {
                    tilesToSkip.Add(coords.GetHashCode());
                    continue;
                }

                tileGraphics = MutationTarget.TheaterGraphics.GetTileGraphics(cell.TileIndex);
                if (tileGraphics.TileSetId != tileSetId)
                {
                    tilesToSkip.Add(coords.GetHashCode());
                    continue;
                }

                subCellImage = tileGraphics.TMPImages[cell.SubTileIndex];
                if (subCellImage.TmpImage.TerrainType != terrainType)
                {
                    tilesToSkip.Add(coords.GetHashCode());
                    continue;
                }
                    
                // Mark this cell as one to process and nearby tiles as ones to check
                tilesToProcess.Add(coords);

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        if (y == 0 && x == 0)
                            continue;

                        var newCellCoords = new Point2D(x, y) + coords;
                        int hash = newCellCoords.GetHashCode();
                        if (!tilesToSkip.Contains(hash) && !tileCheckHashSet.Contains(hash))
                        {
                            tileCheckHashSet.Add(hash);
                            tilesToCheck.AddLast(newCellCoords);
                        }    
                    }
                }
            }

            // Process tiles
            foreach (Point2D cellCoords in tilesToProcess)
            {
                var cell = MutationTarget.Map.GetTile(cellCoords);
                originalData.Add(new OriginalCellTerrainData(cellCoords, cell.TileIndex, cell.SubTileIndex));

                cell.TileImage = null;
                cell.TileIndex = tile.TileID;
                cell.SubTileIndex = 0;
            }

            undoData = originalData.ToArray();
            MutationTarget.InvalidateMap();
        }

        public override void Undo()
        {
            foreach (var originalTerrainData in undoData)
            {
                var cell = MutationTarget.Map.GetTile(originalTerrainData.CellCoords);
                cell.TileImage = null;
                cell.TileIndex = originalTerrainData.TileIndex;
                cell.SubTileIndex = originalTerrainData.SubTileIndex;
            }

            MutationTarget.InvalidateMap();
        }
    }
}
