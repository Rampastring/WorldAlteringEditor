using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class RaiseGroundCursorAction : CursorAction
    {
        public RaiseGroundCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Raise Ground";

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            var mutation = new RaiseGroundMutation(CursorActionTarget.MutationTarget, cellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
