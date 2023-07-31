using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.Input;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows the user to check the distance between two cells.
    /// </summary>
    public class CheckDistanceCursorAction : CursorAction
    {
        public CheckDistanceCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Check Distance";

        private Point2D? source;
        private Point2D? destination;
        private List<Point2D> pathCellCoords = new List<Point2D>();
        private int pathLength = 0;

        public override bool DrawCellCursor => true;

        public override bool HandlesKeyboardInput => true;

        public override void OnKeyPressed(KeyPressEventArgs e)
        {
            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                e.Handled = true;
                ExitAction();
            }
            else if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.C)
            {
                e.Handled = true;
                source = null;
                destination = null;
                pathCellCoords.Clear();
            }

            base.OnKeyPressed(e);
        }

        public override void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint)
        {
            Color sourceColor = Color.LimeGreen;
            Color destinationColor = Color.Red;
            Color pathColor = Color.Yellow;

            if (source == null)
            {
                DrawText(cellCoords, cameraTopLeftPoint, "Click to select source coordinate, or right-click to exit", sourceColor);
                return;
            }

            Point2D sourceCenterPoint = CellMath.CellCenterPointFromCellCoords(source.Value, CursorActionTarget.Map) - cameraTopLeftPoint;
            Renderer.FillRectangle(GetDrawRectangleForMarker(sourceCenterPoint), sourceColor);

            if (destination == null)
            {
                DrawText(cellCoords, cameraTopLeftPoint, "Click to select destination coordinate\r\n\r\nOr right-click to exit,\r\nor press C to clear", destinationColor);
                return;
            }

            Point2D destinationCenterPoint = CellMath.CellCenterPointFromCellCoords(destination.Value, CursorActionTarget.Map) - cameraTopLeftPoint;
            Renderer.FillRectangle(GetDrawRectangleForMarker(destinationCenterPoint), Color.Red);

            foreach (Point2D pathCell in pathCellCoords)
            {
                Point2D pathCellCenterPoint = CellMath.CellCenterPointFromCellCoords(pathCell, CursorActionTarget.Map) - cameraTopLeftPoint;
                pathCellCenterPoint = pathCellCenterPoint.ScaleBy(CursorActionTarget.Camera.ZoomLevel);

                Renderer.FillRectangle(GetDrawRectangleForMarker(pathCellCenterPoint), Color.Yellow);
            }

            string text = "Path Length In Cells: " + pathLength + "\r\n\r\nRight-click to exit\r\nPress C to clear";
            DrawText(cellCoords, cameraTopLeftPoint, text, pathColor);
        }

        private void DrawText(Point2D cellCoords, Point2D cameraTopLeftPoint, string text, Color textColor)
        {
            DrawText(cellCoords, cameraTopLeftPoint, 60, -150, text, textColor);
        }

        private Rectangle GetDrawRectangleForMarker(Point2D cellCenterPoint)
        {
            int size = (int)(10 * CursorActionTarget.Camera.ZoomLevel);
            return new Rectangle(cellCenterPoint.X - (size / 2), cellCenterPoint.Y - (size / 2), size, size);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            if (source == null)
            {
                source = cellCoords;
            }
            else if (destination == null && source != cellCoords)
            {
                destination = cellCoords;

                pathCellCoords.Clear();
                FormPath(source.Value, destination.Value);
                pathLength = pathCellCoords.Count;

                if (destination.Value.X != source.Value.X && destination.Value.Y != source.Value.Y)
                    FormPath(destination.Value, source.Value); // Let's also draw another approach
            }
        }

        private void FormPath(Point2D source, Point2D destination)
        {
            Point2D currentPoint = source;

            while (true)
            {
                int xDiff = 0;
                int yDiff = 0;

                if (currentPoint.X > destination.X)
                    xDiff--;
                else if (currentPoint.X < destination.X)
                    xDiff++;

                if (currentPoint.Y > destination.Y)
                    yDiff--;
                else if (currentPoint.Y < destination.Y)
                    yDiff++;

                currentPoint = currentPoint + new Point2D(xDiff, yDiff);

                if (currentPoint == destination)
                    break;

                pathCellCoords.Add(currentPoint);
            }
        }
    }
}
