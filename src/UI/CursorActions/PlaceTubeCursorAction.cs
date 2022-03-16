using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.Input;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class PlaceTubeCursorAction : CursorAction
    {
        public PlaceTubeCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override bool HandlesKeyboardInput => true;

        public override bool DrawCellCursor => true;

        private Tube tube;

        public override void PreMapDraw(Point2D cellCoords)
        {
            if (tube != null)
                CursorActionTarget.Map.Tubes.Add(tube);
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            if (tube != null)
                CursorActionTarget.Map.Tubes.RemoveAt(CursorActionTarget.Map.Tubes.Count - 1);
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cellCoords, CursorActionTarget.Map.Size.X) - cameraTopLeftPoint;

            const string text = "Click on cell to draw tunnel.\r\n\r\nENTER to confirm\r\nShift + ENTER to also create opposing tunnel\r\nESC to clear\r\nShift + ESC to exit";
            var textDimensions = Renderer.GetTextDimensions(text, Constants.UIBoldFont);
            int x = cellTopLeftPoint.X - (int)(textDimensions.X - Constants.CellSizeX) / 2;

            Vector2 textPosition = new Vector2(x + 60, cellTopLeftPoint.Y - 150);

            Rectangle textBackgroundRectangle = new Rectangle((int)textPosition.X - Constants.UIEmptySideSpace,
                (int)textPosition.Y - Constants.UIEmptyTopSpace,
                (int)textDimensions.X + Constants.UIEmptySideSpace * 2,
                (int)textDimensions.Y + Constants.UIEmptyBottomSpace + Constants.UIEmptyTopSpace);

            Renderer.FillRectangle(textBackgroundRectangle, UISettings.ActiveSettings.PanelBackgroundColor);
            Renderer.DrawRectangle(textBackgroundRectangle, UISettings.ActiveSettings.PanelBorderColor);

            Renderer.DrawStringWithShadow(text, Constants.UIBoldFont, textPosition, Color.Yellow);
        }

        public override void OnKeyPressed(KeyPressEventArgs e)
        {
            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                TubeRefreshHelper.MapViewRefreshTube(tube, CursorActionTarget.MutationTarget);
                tube = null;

                if (CursorActionTarget.WindowManager.Keyboard.IsShiftHeldDown())
                    ExitAction();

                e.Handled = true;
            }
            else if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.Enter && tube.Directions.Count > 0)
            {
                tube.UnitInitialFacing = tube.Directions[0];
                CursorActionTarget.MutationManager.PerformMutation(new PlaceTubeMutation(CursorActionTarget.MutationTarget, tube));

                if (CursorActionTarget.WindowManager.Keyboard.IsShiftHeldDown())
                {
                    Tube reversedTube = tube.GetReversedTube();
                    CursorActionTarget.MutationManager.PerformMutation(new PlaceTubeMutation(CursorActionTarget.MutationTarget, reversedTube));
                }

                tube = null;
                ExitAction();

                e.Handled = true;
            }
        }

        public override void LeftClick(Point2D cellCoords)
        {
            if (tube == null)
            {
                // Make sure that a tube doesn't already exist on this cell
                if (CursorActionTarget.Map.Tubes.Exists(tb => tb.EntryPoint == cellCoords))
                    return;

                tube = new Tube();
                tube.EntryPoint = cellCoords;
                return;
            }

            Point2D lastCell = tube.EntryPoint;
            tube.Directions.ForEach(td => lastCell = lastCell.NextPointFromTubeDirection(td));

            if (cellCoords == lastCell)
                return;

            // Fetch tube direction for this cell
            // Check each tube direction to see if we find a fitting one

            TubeDirection? nextTubeDirection = null;
            for (int i = (int)TubeDirection.First; i <= (int)TubeDirection.Max; i++)
            {
                TubeDirection tubeDirection = (TubeDirection)i;
                Point2D nextPointCandidate = lastCell.NextPointFromTubeDirection(tubeDirection);

                if (nextPointCandidate == cellCoords)
                {
                    nextTubeDirection = tubeDirection;
                    break;
                }
            }

            // If we didn't find a fitting direction, the user didn't press the cursor 
            // on a cell next to our previous tube cell. Jump out.
            if (nextTubeDirection == null)
                return;

            tube.Directions.Add(nextTubeDirection.Value);
            tube.ExitPoint = cellCoords;

            TubeRefreshHelper.MapViewRefreshTube(tube, CursorActionTarget.MutationTarget);
        }

        public override void LeftDown(Point2D cellCoords) => LeftClick(cellCoords);
    }
}
