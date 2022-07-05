using System;
using System.Collections.Generic;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that places a terrain tile on the map.
    /// </summary>
    public class PlaceTerrainTileMutation : Mutation
    {
        public PlaceTerrainTileMutation(IMutationTarget mutationTarget, Point2D targetCellCoords, TileImage tile) : base(mutationTarget)
        {
            this.targetCellCoords = targetCellCoords;
            this.tile = tile;
            this.brushSize = mutationTarget.BrushSize;
        }

        private readonly Point2D targetCellCoords;
        private readonly TileImage tile;
        private readonly BrushSize brushSize;
        private List<OriginalTerrainData> undoData;

        private static readonly Point2D[] surroundingTiles = new Point2D[] { new Point2D(-1, 0), new Point2D(1, 0), new Point2D(0, -1), new Point2D(0, 1) };

        private void AddUndoDataForTile(Point2D brushOffset)
        {
            for (int i = 0; i < tile.TMPImages.Length; i++)
            {
                MGTMPImage image = tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = targetCellCoords.X + (brushOffset.X * tile.Width) + i % tile.Width;
                int cy = targetCellCoords.Y + (brushOffset.Y * tile.Height) + i / tile.Width;

                var mapTile = MutationTarget.Map.GetTile(cx, cy);
                if (mapTile != null && (!MutationTarget.OnlyPaintOnClearGround || mapTile.IsClearGround()) &&
                    !undoData.Exists(otd => otd.CellCoords.X == cx && otd.CellCoords.Y == cy))
                {
                    undoData.Add(new OriginalTerrainData(mapTile.TileIndex, mapTile.SubTileIndex, mapTile.CoordsToPoint()));
                }
            }
        }

        public override void Perform()
        {
            undoData = new List<OriginalTerrainData>(tile.TMPImages.Length * brushSize.Width * brushSize.Height);

            // Get un-do data
            if (MutationTarget.AutoLATEnabled)
            {
                brushSize.DoForBrushSizeAndSurroundings(offset =>
                {
                    AddUndoDataForTile(offset);
                });
            }
            else
            {
                // We don't need to include the surrounding tiles when AutoLAT is disabled
                brushSize.DoForBrushSize(offset =>
                {
                    AddUndoDataForTile(offset);
                });
            }

            // Place the terrain 
            brushSize.DoForBrushSize(offset =>
            {
                for (int i = 0; i < tile.TMPImages.Length; i++)
                {
                    MGTMPImage image = tile.TMPImages[i];

                    if (image.TmpImage == null)
                        continue;

                    int cx = targetCellCoords.X + (offset.X * tile.Width) + i % tile.Width;
                    int cy = targetCellCoords.Y + (offset.Y * tile.Height) + i / tile.Width;

                    var mapTile = MutationTarget.Map.GetTile(cx, cy);
                    if (mapTile != null && (!MutationTarget.OnlyPaintOnClearGround || mapTile.IsClearGround()))
                    {
                        mapTile.TileImage = null;
                        mapTile.TileIndex = tile.TileID;
                        mapTile.SubTileIndex = (byte)i;
                    }
                }
            });

            // Apply autoLAT if necessary
            if (MutationTarget.AutoLATEnabled)
            {
                ApplyAutoLAT();
            }

            MutationTarget.AddRefreshPoint(targetCellCoords, Math.Max(tile.Width, tile.Height) * Math.Max(brushSize.Width, brushSize.Height));
        }

        private void ApplyAutoLAT()
        {
            // Get potential base tilesets of the placed LAT (if we're placing LAT)
            // This allows placing certain LATs on top of other LATs (example: snowy dirt on snow, when snow is also placed on grass)
            TileSet baseTileSet = null;
            TileSet altBaseTileSet = null;
            var tileAutoLatGround = MutationTarget.Map.TheaterInstance.Theater.LATGrounds.Find(
                g => g.GroundTileSet.Index == tile.TileSetId || g.TransitionTileSet.Index == tile.TileSetId);

            if (tileAutoLatGround != null && tileAutoLatGround.BaseTileSet != null)
            {
                int baseTileSetId = tileAutoLatGround.BaseTileSet.Index;
                var baseLatGround = MutationTarget.Map.TheaterInstance.Theater.LATGrounds.Find(
                    g => g.GroundTileSet.Index == baseTileSetId || g.TransitionTileSet.Index == baseTileSetId);

                if (baseLatGround != null)
                {
                    baseTileSet = baseLatGround.GroundTileSet;
                    altBaseTileSet = baseLatGround.TransitionTileSet;
                }
            }

            brushSize.DoForBrushSizeAndSurroundings(offset =>
            {
                var mapTile = MutationTarget.Map.GetTile(targetCellCoords + offset);
                if (mapTile == null)
                    return;

                int x = mapTile.X;
                int y = mapTile.Y;

                int tileSetIndex = MutationTarget.Map.TheaterInstance.GetTileSetId(mapTile.TileIndex);

                var latGrounds = MutationTarget.Map.TheaterInstance.Theater.LATGrounds;

                // If we're not on a tile can be auto-LAT'd in the first place, skip
                var ourLatGround = latGrounds.Find(lg => lg.GroundTileSet.Index == tileSetIndex || lg.TransitionTileSet.Index == tileSetIndex);
                if (ourLatGround == null)
                    return;

                // For Auto-LAT purposes, consider our current tile set not to be transitional if it is so
                var matchingLatGround = latGrounds.Find(lg => lg.TransitionTileSet.Index == tileSetIndex);
                if (matchingLatGround != null)
                    tileSetIndex = matchingLatGround.GroundTileSet.Index;

                // Don't auto-lat ground that is a base for our placed ground type
                if ((baseTileSet != null && tileSetIndex == baseTileSet.Index) ||
                (altBaseTileSet != null && tileSetIndex == altBaseTileSet.Index))
                    return;

                // Look at the surrounding tiles to figure out the base tile set ID we should use
                int baseTileSetId = -1;

                foreach (var otherTileOffset in surroundingTiles)
                {
                    var otherTile = MutationTarget.Map.GetTile(x + otherTileOffset.X, y + otherTileOffset.Y);
                    if (otherTile == null)
                        continue;

                    int otherTileSetId = MutationTarget.Map.TheaterInstance.GetTileSetId(otherTile.TileIndex);
                    if (otherTileSetId != tileSetIndex)
                    {
                        // Check that the other tile is not a transitional tile type
                        var otherLatGround = latGrounds.Find(lg => lg.TransitionTileSet.Index == otherTileSetId);

                        if (otherLatGround == null)
                        {
                            if (otherTileSetId == 0 || latGrounds.Exists(lg => lg.BaseTileSet.Index == otherTileSetId))
                            {
                                baseTileSetId = otherTileSetId;
                                break;
                            }
                            else if (otherTileSetId != 0 && !latGrounds.Exists(lg => lg.BaseTileSet.Index == otherTileSetId))
                            {
                                baseTileSetId = 0;
                                continue;
                            }
                        }
                        else
                        {
                            // If it is a transitional tile type, then take its base tile set for our base tile set
                            // .. UNLESS we can connect to the transition smoothly as indicated by the non-transition
                            // ground tileset of the other cell's LAT being our base tileset,
                            // then take the actual non-transition ground for our base
                            if (ourLatGround.BaseTileSet == otherLatGround.GroundTileSet)
                                baseTileSetId = otherLatGround.GroundTileSet.Index;
                            else
                                baseTileSetId = otherLatGround.BaseTileSet.Index;

                            break;
                        }
                    }
                }

                if (baseTileSetId == -1)
                {
                    // Based on the surrounding tiles, we shouldn't need to use any base tile set
                    mapTile.TileIndex = MutationTarget.Map.TheaterInstance.Theater.TileSets[tileSetIndex].StartTileIndex;
                    mapTile.SubTileIndex = 0;
                    mapTile.TileImage = null;
                    return;
                }

                var autoLatGround = latGrounds.Find(g => g.GroundTileSet.Index == tileSetIndex &&
                    g.TransitionTileSet.Index != baseTileSetId && g.BaseTileSet.Index == baseTileSetId);

                var tileSet = MutationTarget.Map.TheaterInstance.Theater.TileSets[tileSetIndex];

                // When applying auto-LAT to an alt. terrain tile set, don't apply a transition when we are
                // evaluating a base alt. terrain tile set next to ground that is supposed on place on that
                // alt. terrain
                // For example, ~~~Snow shouldn't be auto-LAT'd when it's next to a tile belonging to ~~~Straight Dirt Roads
                Func<TileSet, bool> miscChecker = null;
                if (tileSet.SetName.StartsWith("~~~") && latGrounds.Exists(g => g.BaseTileSet == tileSet))
                {
                    miscChecker = (ts) =>
                    {
                        // On its own line so it's possible to debug this
                        return ts.SetName.StartsWith("~~~") && !latGrounds.Exists(g => g.GroundTileSet == ts);
                    };
                }

                if (autoLatGround != null)
                {
                    int autoLatIndex = MutationTarget.Map.GetAutoLATIndex(mapTile, autoLatGround.GroundTileSet.Index, autoLatGround.TransitionTileSet.Index, miscChecker);

                    if (autoLatIndex == -1)
                    {
                        // There's an edge case where this code path is hit unintentionally
                        mapTile.TileIndex = autoLatGround.GroundTileSet.StartTileIndex;
                    }
                    else
                    {
                        mapTile.TileIndex = autoLatGround.TransitionTileSet.StartTileIndex + autoLatIndex;
                    }

                    mapTile.SubTileIndex = 0;
                    mapTile.TileImage = null;
                }
            });
        }

        public override void Undo()
        {
            for (int i = 0; i < undoData.Count; i++)
            {
                OriginalTerrainData originalTerrainData = undoData[i];

                var mapCell = MutationTarget.Map.GetTile(originalTerrainData.CellCoords);
                if (mapCell != null)
                {
                    mapCell.ChangeTileIndex(originalTerrainData.TileIndex, originalTerrainData.SubTileIndex);
                }
            }

            MutationTarget.AddRefreshPoint(targetCellCoords);
        }
    }
}
