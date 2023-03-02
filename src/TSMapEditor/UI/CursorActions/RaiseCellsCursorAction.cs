using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    internal class RaiseCellsCursorAction : CursorAction
    {
        public RaiseCellsCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            Point2D targetCellCoords = cellCoords + new Point2D(CursorActionTarget.BrushSize.Width / 2, CursorActionTarget.BrushSize.Height / 2);

            var mutation = new RaiseCellsMutation(CursorActionTarget.MutationTarget, targetCellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
