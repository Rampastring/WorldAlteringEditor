using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    public struct CopiedTerrainData
    {
        public int TileIndex;
        public byte SubTileIndex;
        public Point2D Offset;

        public CopiedTerrainData(int tileIndex, byte subTileIndex, Point2D offset)
        {
            TileIndex = tileIndex;
            SubTileIndex = subTileIndex;
            Offset = offset;
        }
    }

    /// <summary>
    /// A mutation that allows pasting terrain on the map.
    /// </summary>
    public class PasteTerrainMutation : Mutation
    {
        public PasteTerrainMutation(IMutationTarget mutationTarget, List<CopiedTerrainData> copiedTerrain, Point2D origin) : base(mutationTarget)
        {
            this.copiedTerrain = copiedTerrain;
            this.origin = origin;
        }

        private readonly List<CopiedTerrainData> copiedTerrain;
        private readonly Point2D origin;

        private OriginalCellTerrainData[] undoData;

        private void AddRefresh()
        {
            if (copiedTerrain.Count > 10)
                MutationTarget.InvalidateMap();
            else
                MutationTarget.AddRefreshPoint(origin);
        }

        public override void Perform()
        {
            var undoData = new List<OriginalCellTerrainData>();

            foreach (var copiedTerrainData in copiedTerrain)
            {
                Point2D cellCoords = origin + copiedTerrainData.Offset;
                MapTile cell = MutationTarget.Map.GetTile(cellCoords);

                if (cell == null)
                    continue;

                undoData.Add(new OriginalCellTerrainData(cellCoords, cell.TileIndex, cell.SubTileIndex));

                cell.TileImage = null;
                cell.TileIndex = copiedTerrainData.TileIndex;
                cell.SubTileIndex = copiedTerrainData.SubTileIndex;
            }

            this.undoData = undoData.ToArray();

            AddRefresh();
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

            AddRefresh();
        }
    }
}
