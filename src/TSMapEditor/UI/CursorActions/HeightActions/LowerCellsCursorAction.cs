using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes.HeightMutations;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions.HeightActions
{
    internal class LowerCellsCursorAction : CursorAction
    {
        public LowerCellsCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Lower Cells";

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            Point2D targetCellCoords = cellCoords - new Point2D(CursorActionTarget.BrushSize.Width / 2, CursorActionTarget.BrushSize.Height / 2);

            var mutation = new LowerCellsMutation(CursorActionTarget.MutationTarget, targetCellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
