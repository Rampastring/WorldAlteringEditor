using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public class OverlayRenderer : ObjectRenderer<Overlay>
    {
        public OverlayRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => new Color(255, 0, 255);

        protected override CommonDrawParams GetDrawParams(Overlay gameObject)
        {
            return new CommonDrawParams(TheaterGraphics.OverlayTextures[gameObject.OverlayType.Index], gameObject.OverlayType.ININame);
        }

        protected override void Render(Overlay gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            int tiberiumIndex = gameObject.OverlayType.GetTiberiumIndex();

            Color remapColor = Color.White;
            if (tiberiumIndex > -1 && tiberiumIndex < Map.Rules.TiberiumTypes.Count)
                remapColor = Map.Rules.TiberiumTypes[tiberiumIndex].XNAColor;

            DrawShadow(gameObject, commonDrawParams, drawPoint, yDrawPointWithoutCellHeight);
            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, gameObject.FrameIndex, Color.White, true, remapColor, drawPoint, yDrawPointWithoutCellHeight);
        }
    }
}
