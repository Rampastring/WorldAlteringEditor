using System;
using System.Collections.Generic;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
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

        struct OriginalTerrainData
        {
            public OriginalTerrainData(int tileIndex, int subTileIndex, Point2D cellCoords)
            {
                TileIndex = tileIndex;
                SubTileIndex = subTileIndex;
                CellCoords = cellCoords;
            }

            public int TileIndex;
            public int SubTileIndex;
            public Point2D CellCoords;
        }

        private readonly Point2D targetCellCoords;
        private readonly TileImage tile;
        private readonly BrushSize brushSize;
        private List<OriginalTerrainData> undoData;

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
                if (mapTile != null && !undoData.Exists(otd => otd.CellCoords.X == cx && otd.CellCoords.Y == cy))
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
                    if (mapTile != null)
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
                    // Don't auto-lat ground that is a base for our placed ground type
                    if ((baseTileSet != null && tileSetIndex == baseTileSet.Index) ||
                        (altBaseTileSet != null && tileSetIndex == altBaseTileSet.Index))
                        return;

                    var autoLatGround = MutationTarget.Map.TheaterInstance.Theater.LATGrounds.Find(g => g.GroundTileSet.Index == tileSetIndex || g.TransitionTileSet.Index == tileSetIndex);
                    if (autoLatGround != null)
                    {
                        int autoLatIndex = MutationTarget.Map.GetAutoLATIndex(mapTile, autoLatGround.GroundTileSet.Index, autoLatGround.TransitionTileSet.Index);
                        if (autoLatIndex == -1)
                        {
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

            MutationTarget.AddRefreshPoint(targetCellCoords, Math.Max(tile.Width, tile.Height) * Math.Max(brushSize.Width, brushSize.Height));
        }

        public override void Undo()
        {
            for (int i = 0; i < undoData.Count; i++)
            {
                OriginalTerrainData originalTerrainData = undoData[i];

                var mapTile = MutationTarget.Map.GetTile(originalTerrainData.CellCoords);
                if (mapTile != null)
                {
                    mapTile.TileImage = null;
                    mapTile.TileIndex = originalTerrainData.TileIndex;
                    mapTile.SubTileIndex = (byte)originalTerrainData.SubTileIndex;
                }
            }

            MutationTarget.AddRefreshPoint(targetCellCoords);
        }
    }
}
