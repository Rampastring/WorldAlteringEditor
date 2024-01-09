using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public sealed class InfantryRenderer : ObjectRenderer<Infantry>
    {
        public InfantryRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Teal;

        protected override CommonDrawParams GetDrawParams(Infantry gameObject)
        {
            var graphics = TheaterGraphics.InfantryTextures[gameObject.ObjectType.Index];
            string iniName = gameObject.ObjectType.ININame;

            return new CommonDrawParams(graphics, iniName);
        }

        protected override void Render(Infantry gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            switch (gameObject.SubCell)
            {
                case SubCell.Top:
                    drawPoint += new Point2D(0, Constants.CellSizeY / -4);
                    break;
                case SubCell.Bottom:
                    drawPoint += new Point2D(0, Constants.CellSizeY / 4);
                    break;
                case SubCell.Left:
                    drawPoint += new Point2D(Constants.CellSizeX / -4, 0);
                    break;
                case SubCell.Right:
                    drawPoint += new Point2D(Constants.CellSizeX / 4, 0);
                    break;
                case SubCell.Center:
                default:
                    break;
            }

            if (!gameObject.ObjectType.NoShadow)
                DrawShadow(gameObject, commonDrawParams, drawPoint, yDrawPointWithoutCellHeight);

            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, 
                gameObject.GetFrameIndex(commonDrawParams.Graphics.Frames.Length), 
                Color.White, true, gameObject.GetRemapColor(), drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
