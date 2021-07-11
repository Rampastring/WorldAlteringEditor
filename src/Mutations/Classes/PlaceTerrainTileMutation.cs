using System;
using System.Collections.Generic;
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
        public PlaceTerrainTileMutation(IMutationTarget mutationTarget, MapTile target, TileImage tile) : base(mutationTarget)
        {
            this.target = target;
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

        private readonly MapTile target;
        private readonly TileImage tile;
        private readonly BrushSize brushSize;
        private OriginalTerrainData[] undoData;


        public override void Perform()
        {
            var undoData = new List<OriginalTerrainData>();

            brushSize.DoForBrushSize(offset =>
            {
                for (int i = 0; i < tile.TMPImages.Length; i++)
                {
                    MGTMPImage image = tile.TMPImages[i];

                    if (image.TmpImage == null)
                        continue;

                    int cx = target.X + (offset.X * tile.Width) + i % tile.Width;
                    int cy = target.Y + (offset.Y * tile.Height) + i / tile.Width;

                    var mapTile = MutationTarget.Map.GetTile(cx, cy);
                    if (mapTile != null)
                    {
                        undoData.Add(new OriginalTerrainData(mapTile.TileIndex, mapTile.SubTileIndex, new Point2D(cx, cy)));

                        mapTile.TileImage = null;
                        mapTile.TileIndex = tile.TileID;
                        mapTile.SubTileIndex = (byte)i;
                    }
                }
            });

            this.undoData = undoData.ToArray();
            MutationTarget.AddRefreshPoint(target.CoordsToPoint(), Math.Max(tile.Width, tile.Height) * Math.Max(brushSize.Width, brushSize.Height));
        }

        public override void Undo()
        {
            for (int i = 0; i < undoData.Length; i++)
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

            MutationTarget.AddRefreshPoint(target.CoordsToPoint());
        }
    }
}
