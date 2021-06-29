using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    class TerrainPlacementAction : CursorAction
    {
        public TileImage Tile { get; set; }

        public override void PreMapDraw(Point2D cellCoordsOnCursor, ICursorActionTarget cursorActionTarget)
        {
            // Assign preview data
            DoActionForCells(cellCoordsOnCursor, cursorActionTarget, t => t.PreviewTileImage = Tile);
        }

        public override void PostMapDraw(Point2D cellCoordsOnCursor, ICursorActionTarget cursorActionTarget)
        {
            // Clear preview data
            DoActionForCells(cellCoordsOnCursor, cursorActionTarget, t => t.PreviewTileImage = null);
        }

        private void DoActionForCells(Point2D cellCoordsOnCursor, ICursorActionTarget cursorActionTarget, Action<MapTile> action)
        {
            if (Tile == null)
                return;

            for (int i = 0; i < Tile.TMPImages.Length; i++)
            {
                MGTMPImage image = Tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = cellCoordsOnCursor.X + i % Tile.Width;
                int cy = cellCoordsOnCursor.Y + i / Tile.Width;

                var mapTile = cursorActionTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    mapTile.PreviewSubTileIndex = i;
                    action(mapTile);
                }
            }

            cursorActionTarget.AddRefreshPoint(cellCoordsOnCursor);
        }

        public override void LeftDown(Point2D cellPoint, ICursorActionTarget cursorActionTarget)
        {
            if (Tile == null)
                return;

            var mutation = new ChangeTerrainMutation(cursorActionTarget.MutationTarget, cursorActionTarget.Map.GetTile(cellPoint), Tile);
            cursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellPoint, ICursorActionTarget cursorActionTarget)
        {
            LeftDown(cellPoint, cursorActionTarget);
        }
    }
}
