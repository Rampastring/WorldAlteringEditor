using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public class SmudgeRenderer : ObjectRenderer<Smudge>
    {
        public SmudgeRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Cyan;

        protected override CommonDrawParams GetDrawParams(Smudge gameObject)
        {
            return new CommonDrawParams(TheaterGraphics.SmudgeTextures[gameObject.SmudgeType.Index], gameObject.SmudgeType.ININame);
        }

        protected override void Render(Smudge gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, 0, Color.White, false, Color.White, drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
