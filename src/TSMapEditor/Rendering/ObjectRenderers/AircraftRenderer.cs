using Microsoft.Xna.Framework;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    internal class AircraftRenderer : ObjectRenderer<Aircraft>
    {
        public AircraftRenderer(RenderDependencies renderDependencies) : base(renderDependencies)
        {
        }

        protected override Color ReplacementColor => Color.HotPink;

        protected override CommonDrawParams GetDrawParams(Aircraft gameObject)
        {
            return new CommonDrawParams(null, gameObject.ObjectType.ININame);
        }

        protected override void Render(Aircraft gameObject, int yDrawPointWithoutCellHeight, Point2D drawPoint, CommonDrawParams commonDrawParams)
        {
            // lol, this is an easy one
            throw new NotImplementedException();
        }
    }
}
