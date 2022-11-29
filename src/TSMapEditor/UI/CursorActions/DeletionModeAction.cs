using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows the user to delete anything.
    /// </summary>
    public class DeletionModeAction : CursorAction
    {
        public DeletionModeAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cellCoords, CursorActionTarget.Map) - cameraTopLeftPoint;
            cellTopLeftPoint = cellTopLeftPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);

            const string text = "Delete";
            var textDimensions = Renderer.GetTextDimensions(text, Constants.UIBoldFont);
            int x = cellTopLeftPoint.X - (int)(textDimensions.X - Constants.CellSizeX) / 2;

            Renderer.DrawStringWithShadow(text,
                Constants.UIBoldFont,
                new Vector2(x, cellTopLeftPoint.Y),
                Color.Red);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            CursorActionTarget.Map.DeleteObjectFromCell(cellCoords);
            CursorActionTarget.AddRefreshPoint(cellCoords);
        }

        public override void LeftDown(Point2D cellCoords) => LeftClick(cellCoords);
    }
}
