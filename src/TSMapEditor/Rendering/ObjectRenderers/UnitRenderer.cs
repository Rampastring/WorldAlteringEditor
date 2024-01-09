using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public sealed class UnitRenderer : ObjectRenderer<Unit>
    {
        public UnitRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Red;

        protected override CommonDrawParams GetDrawParams(Unit gameObject)
        {
            var graphics = TheaterGraphics.UnitTextures[gameObject.ObjectType.Index];
            string iniName = gameObject.ObjectType.ININame;
            return new CommonDrawParams(graphics,  iniName);
        }

        protected override void Render(Unit gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            if (!gameObject.ObjectType.NoShadow)
                DrawShadow(gameObject, commonDrawParams, drawPoint, yDrawPointWithoutCellHeight);

            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, 
                gameObject.GetFrameIndex(commonDrawParams.Graphics.Frames.Length),
                Color.White, true, gameObject.GetRemapColor(), drawPoint, yDrawPointWithoutCellHeight);

            if (gameObject.UnitType.Turret)
            {
                int turretFrameIndex = gameObject.GetTurretFrameIndex();
                if (turretFrameIndex > -1 && turretFrameIndex < commonDrawParams.Graphics.Frames.Length)
                {
                    PositionedTexture frame = commonDrawParams.Graphics.Frames[turretFrameIndex];

                    if (frame == null)
                        return;

                    DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics, 
                        turretFrameIndex, Color.White, true, gameObject.GetRemapColor(),
                        drawPoint, yDrawPointWithoutCellHeight);
                }
            }
        }
    }
}
