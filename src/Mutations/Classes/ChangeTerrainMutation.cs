using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that changes the terrain of the map.
    /// </summary>
    public class ChangeTerrainMutation : Mutation
    {
        public ChangeTerrainMutation(IMutationTarget mutationTarget, MapTile target, TileImage tile) : base(mutationTarget)
        {
            this.target = target;
            this.tile = tile;
        }

        struct OriginalTerrainData
        {
            public OriginalTerrainData(int tileIndex, int subTileIndex)
            {
                TileIndex = tileIndex;
                SubTileIndex = subTileIndex;
            }

            public int TileIndex;
            public int SubTileIndex;
        }

        private readonly MapTile target;
        private readonly TileImage tile;
        private OriginalTerrainData[] undoData;


        public override void Perform()
        {
            var undoData = new List<OriginalTerrainData>();

            for (int i = 0; i < tile.TMPImages.Length; i++)
            {
                MGTMPImage image = tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = target.X + i % tile.Width;
                int cy = target.Y + i / tile.Width;

                var mapTile = MutationTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    undoData.Add(new OriginalTerrainData(mapTile.TileIndex, mapTile.SubTileIndex));

                    mapTile.TileImage = null;
                    mapTile.TileIndex = tile.TileID;
                    mapTile.SubTileIndex = (byte)i;
                }
            }

            this.undoData = undoData.ToArray();
            MutationTarget.AddRefreshPoint(target.CoordsToPoint());
        }

        public override void Undo()
        {
            int undoDataIndex = 0;
            for (int i = 0; i < tile.TMPImages.Length; i++)
            {
                MGTMPImage image = tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = target.X + i % tile.Width;
                int cy = target.Y + i / tile.Width;

                var mapTile = MutationTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    mapTile.TileImage = null;
                    mapTile.TileIndex = undoData[undoDataIndex].TileIndex;
                    mapTile.SubTileIndex = (byte)undoData[undoDataIndex].SubTileIndex;

                    undoDataIndex++;
                }
            }

            MutationTarget.AddRefreshPoint(target.CoordsToPoint());
        }
    }
}
