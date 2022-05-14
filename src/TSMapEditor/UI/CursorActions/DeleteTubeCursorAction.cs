using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class DeleteTubeCursorAction : CursorAction
    {
        public DeleteTubeCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            var tube = CursorActionTarget.Map.Tubes.Find(tb => tb.EntryPoint == cellCoords || tb.ExitPoint == cellCoords);
            Color color = tube == null ? Color.Gray : Color.Red;

            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cellCoords, CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;

            const string text = "Delete Tunnel";
            var textDimensions = Renderer.GetTextDimensions(text, Constants.UIBoldFont);
            int x = cellTopLeftPoint.X - (int)(textDimensions.X - Constants.CellSizeX) / 2;

            Renderer.DrawStringWithShadow(text,
                Constants.UIBoldFont,
                new Vector2(x, cellTopLeftPoint.Y),
                color);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            var tube = CursorActionTarget.Map.Tubes.Find(tb => tb.EntryPoint == cellCoords || tb.ExitPoint == cellCoords);
            if (tube == null)
                return;

            CursorActionTarget.MutationManager.PerformMutation(new DeleteTubeMutation(CursorActionTarget.MutationTarget, cellCoords));
        }

        public override void LeftDown(Point2D cellCoords) => LeftClick(cellCoords);
    }
}
