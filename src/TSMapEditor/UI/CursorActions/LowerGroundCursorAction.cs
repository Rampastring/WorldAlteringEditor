using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    internal class LowerGroundCursorAction : CursorAction
    {
        public LowerGroundCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Lower Ground";

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            var mutation = new LowerGroundMutation(CursorActionTarget.MutationTarget, cellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
