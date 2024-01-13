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

        protected override ICommonDrawParams GetDrawParams(Smudge gameObject)
        {
            return new ShapeDrawParams(TheaterGraphics.SmudgeTextures[gameObject.SmudgeType.Index], gameObject.SmudgeType.ININame);
        }

        protected override void Render(Smudge gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, ICommonDrawParams drawParams)
        {
            if (drawParams is not ShapeDrawParams shapeDrawParams)
                return;

            DrawShapeImage(gameObject, shapeDrawParams, shapeDrawParams.Graphics, 0, Color.White, false, Color.White, drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
