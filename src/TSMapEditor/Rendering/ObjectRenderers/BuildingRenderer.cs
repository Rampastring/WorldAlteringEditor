using Microsoft.Xna.Framework;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    public sealed class BuildingRenderer : ObjectRenderer<Structure>
    {
        public BuildingRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.Yellow;

        private void DrawFoundationLines(Structure gameObject)
        {
            int foundationX = gameObject.ObjectType.ArtConfig.FoundationX;
            int foundationY = gameObject.ObjectType.ArtConfig.FoundationY;

            Color foundationLineColor = gameObject.Owner.XNAColor;

            if (foundationX > 0 && foundationY > 0)
            {
                var map = RenderDependencies.Map;

                int heightOffset = 0;

                var cell = map.GetTile(gameObject.Position);
                if (cell != null)
                    heightOffset = cell.Level * Constants.CellHeight;

                Point2D p1 = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, map) + new Point2D(Constants.CellSizeX / 2, 0);
                Point2D p2 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y), map) + new Point2D(Constants.CellSizeX / 2, 0);
                Point2D p3 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X, gameObject.Position.Y + foundationY), map) + new Point2D(Constants.CellSizeX / 2, 0);
                Point2D p4 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y + foundationY), map) + new Point2D(Constants.CellSizeX / 2, 0);

                p1 -= new Point2D(0, heightOffset);
                p2 -= new Point2D(0, heightOffset);
                p3 -= new Point2D(0, heightOffset);
                p4 -= new Point2D(0, heightOffset);

                SetEffectParams(0.0f, 0.0f, Vector2.Zero, Vector2.Zero);

                DrawLine(p1.ToXNAVector(), p2.ToXNAVector(), foundationLineColor, 1);
                DrawLine(p1.ToXNAVector(), p3.ToXNAVector(), foundationLineColor, 1);
                DrawLine(p2.ToXNAVector(), p4.ToXNAVector(), foundationLineColor, 1);
                DrawLine(p3.ToXNAVector(), p4.ToXNAVector(), foundationLineColor, 1);
            }
        }

        protected override CommonDrawParams GetDrawParams(Structure gameObject)
        {
            var graphics = RenderDependencies.TheaterGraphics.BuildingTextures[gameObject.ObjectType.Index];
            string iniName = gameObject.ObjectType.ININame;

            return new CommonDrawParams(graphics, iniName);
        }

        protected override bool ShouldRenderReplacementText(Structure gameObject)
        {
            var bibGraphics = RenderDependencies.TheaterGraphics.BuildingBibTextures[gameObject.ObjectType.Index];

            if (bibGraphics != null)
                return false;

            return base.ShouldRenderReplacementText(gameObject);
        }

        private void DrawBibGraphics(Structure gameObject, ObjectImage bibGraphics, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            DrawObjectImage(gameObject, commonDrawParams, bibGraphics, 0, Color.White, true, gameObject.GetRemapColor(), drawPoint, yDrawPointWithoutCellHeight);
        }

        protected override void Render(Structure gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            DrawFoundationLines(gameObject);

            var bibGraphics = RenderDependencies.TheaterGraphics.BuildingBibTextures[gameObject.ObjectType.Index];
            
            if (bibGraphics != null)
                DrawBibGraphics(gameObject, bibGraphics, yDrawPointWithoutCellHeight, drawPoint, commonDrawParams);

            DrawShadow(gameObject, commonDrawParams, drawPoint, yDrawPointWithoutCellHeight);

            DrawObjectImage(gameObject, commonDrawParams, commonDrawParams.Graphics,
                gameObject.GetFrameIndex(commonDrawParams.Graphics.Frames.Length),
                Color.White, true, gameObject.GetRemapColor(), drawPoint, yDrawPointWithoutCellHeight);
        }

        protected override void DrawObjectReplacementText(Structure gameObject, CommonDrawParams drawParams, Point2D drawPoint)
        {
            DrawFoundationLines(gameObject);

            base.DrawObjectReplacementText(gameObject, drawParams, drawPoint);
        }
    }
}
