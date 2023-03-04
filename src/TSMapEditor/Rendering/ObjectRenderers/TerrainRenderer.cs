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

        protected override CommonDrawParams GetDrawParams(TerrainObject gameObject)
        {
            return new CommonDrawParams(TheaterGraphics.TerrainObjectTextures[gameObject.TerrainType.Index], gameObject.TerrainType.ININame);
        }

        protected override void Render(TerrainObject gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            DrawShadow(gameObject, commonDrawParams, drawPoint, yDrawPointWithoutCellHeight);

            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, 0, 
                Color.White, false, Color.White, drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
