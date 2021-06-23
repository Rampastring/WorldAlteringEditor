using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    class TerrainPlacementAction : CursorAction
    {
        public TileImage Tile { get; set; }

        public override void LeftDown(Point2D cellPoint, ICursorActionTarget cursorActionTarget)
        {
            base.LeftDown(cellPoint, cursorActionTarget);
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
