using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public sealed class TerrainRenderer : ObjectRenderer<TerrainObject>
    {
        public TerrainRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Green;

        protected override ICommonDrawParams GetDrawParams(TerrainObject gameObject)
        {
            return new ShapeDrawParams(TheaterGraphics.TerrainObjectTextures[gameObject.TerrainType.Index], gameObject.TerrainType.ININame);
        }

        protected override void Render(TerrainObject gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, ICommonDrawParams drawParams)
        {
            if (drawParams is not ShapeDrawParams shapeDrawParams)
                return;

            DrawShadow(gameObject, shapeDrawParams, drawPoint, yDrawPointWithoutCellHeight);

            DrawShapeImage(gameObject, shapeDrawParams, shapeDrawParams.Graphics, 0, 
                Color.White, false, Color.White, drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
