using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    internal class LowerCellsCursorAction : CursorAction
    {
        public LowerCellsCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            var mutation = new LowerCellsMutation(CursorActionTarget.MutationTarget, cellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
