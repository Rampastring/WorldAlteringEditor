using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
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

            cursorActionTarget.AddRefreshPoint(CellMath.CellTopLeftPoint(cellCoordsOnCursor, cursorActionTarget.Map.Size.X));
        }

        public override void LeftDown(Point2D cellPoint, ICursorActionTarget cursorActionTarget)
        {
            if (Tile == null)
                return;

            for (int i = 0; i < Tile.TMPImages.Length; i++)
            {
                if (Tile.TMPImages[i].TmpImage == null)
                    continue;

                int cx = cellPoint.X + i % Tile.Width;
                int cy = cellPoint.Y + i / Tile.Width;

                var mapTile = cursorActionTarget.Map.GetTile(cx, cy);
                if (mapTile != null)
                {
                    mapTile.TileImage = null;
                    mapTile.TileIndex = Tile.TileID;
                    mapTile.SubTileIndex = (byte)i;
                }
            }

            cursorActionTarget.AddRefreshPoint(CellMath.CellTopLeftPoint(cellPoint, cursorActionTarget.Map.Size.X));
        }

        public override void DrawPreview(Point2D cellTopLeftPoint, ICursorActionTarget cursorActionTarget)
        {
            if (Tile == null)
                return;

            foreach (MGTMPImage image in Tile.TMPImages)
            {
                if (image == null || image.TmpImage == null)
                    continue;

                // TODO separate map into terrain and object layers so we can draw on top of the terrain layer
                // cursorActionTarget.Map.
                Renderer.DrawTexture(image.Texture, new Rectangle(cellTopLeftPoint.X + image.TmpImage.X,
                    cellTopLeftPoint.Y + image.TmpImage.Y,
                    Constants.CellSizeX, Constants.CellSizeY), Color.White);
            }
        }

        public override void LeftClick(Point2D cellPoint, ICursorActionTarget cursorActionTarget)
        {
            throw new NotImplementedException();
        }
    }
}
