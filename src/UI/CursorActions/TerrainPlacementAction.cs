using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    class TerrainPlacementAction : CursorAction
    {
        public TerrainPlacementAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public TileImage Tile { get; set; }

        public override void PreMapDraw(Point2D cellCoords)
        {
            // Assign preview data
            DoActionForCells(cellCoords, t => t.PreviewTileImage = Tile);
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            // Clear preview data
            DoActionForCells(cellCoords, t => t.PreviewTileImage = null);
        }

        private void DoActionForCells(Point2D cellCoords, Action<MapTile> action)
        {
            if (Tile == null)
                return;

            for (int i = 0; i < Tile.TMPImages.Length; i++)
            {
                MGTMPImage image = Tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = cellCoords.X + i % Tile.Width;
                int cy = cellCoords.Y + i / Tile.Width;

                var mapTile = CursorActionTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    mapTile.PreviewSubTileIndex = i;
                    action(mapTile);
                }
            }

            CursorActionTarget.AddRefreshPoint(cellCoords);
        }

        public override void LeftDown(Point2D cellCoords)
        {
            if (Tile == null)
                return;

            var mutation = new ChangeTerrainMutation(CursorActionTarget.MutationTarget, CursorActionTarget.Map.GetTile(cellCoords), Tile);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            LeftDown(cellCoords);
        }
    }
}
