using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes.HeightMutations;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions.HeightActions
{
    /// <summary>
    /// Non-steep, "FinalSun-like" ground raising cursor action.
    /// </summary>
    public class FSRaiseGroundCursorAction : CursorAction
    {
        public FSRaiseGroundCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Raise Ground (Non-Steep Ramps)";

        public override bool DrawCellCursor => true;

        public override void LeftClick(Point2D cellCoords)
        {
            base.LeftClick(cellCoords);

            var mutation = new FSRaiseGroundMutation(CursorActionTarget.MutationTarget, cellCoords, CursorActionTarget.BrushSize);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
