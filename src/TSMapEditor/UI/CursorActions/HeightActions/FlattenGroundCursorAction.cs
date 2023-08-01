using TSMapEditor.GameMath;
using TSMapEditor.Mutations.Classes.HeightMutations;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions.HeightActions
{
    /// <summary>
    /// A cursor action that allows the user to "flatten" ground by holding
    /// the left mouse button and moving it over the map.
    /// 
    /// The cell that the cursor first was on when the user started holding
    /// the mouse button tells which height level which gets assigned to
    /// all other cells that the cursor passes through.
    /// </summary>
    public class FlattenGroundCursorAction : CursorAction
    {
        public FlattenGroundCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        private int desiredHeightLevel = -1;

        public override string GetName() => "Flatten Ground";

        public override bool DrawCellCursor => true;

        public override void LeftDown(Point2D cellCoords)
        {
            var cell = Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (desiredHeightLevel == -1)
            {
                // This is the first cell that the user is holding the cursor on,
                // meaning it's the origin cell that determines the height level
                // that the user wants to assign to other cells
                desiredHeightLevel = cell.Level;
                return;
            }

            if (cell.Level == desiredHeightLevel)
            {
                return;
            }

            // Don't act on non-clear terrain
            if (!cell.IsClearGround() && !Map.TheaterInstance.Theater.RampTileSet.ContainsTile(cell.TileIndex))
            {
                return;
            }

            CursorActionTarget.MutationManager.PerformMutation(new FlattenGroundMutation(MutationTarget, cellCoords, CursorActionTarget.BrushSize, desiredHeightLevel));
        }

        public override void LeftUpOnMouseMove(Point2D cellCoords)
        {
            desiredHeightLevel = -1;
        }
    }
}
