using System;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    public abstract class CursorAction
    {
        public CursorAction(ICursorActionTarget cursorActionTarget)
        {
            CursorActionTarget = cursorActionTarget;
        }

        /// <summary>
        /// Raised when the cursor action is exited. 
        /// Typically this happens through the user right-clicking or 
        /// through the activation of a different cursor action.
        /// </summary>
        public event EventHandler ActionExited;

        public void ExitAction() => ActionExited?.Invoke(this, EventArgs.Empty);

        protected ICursorActionTarget CursorActionTarget { get; }

        /// <summary>
        /// Called prior to drawing the map.
        /// </summary>
        /// <param name="cellCoords">The coords of the cell under the cursor.</param>
        public virtual void PreMapDraw(Point2D cellCoords) { }

        /// <summary>
        /// Called after drawing the map.
        /// Override in derived classes to clear preview data related to the action.
        /// </summary>
        /// <param name="cellCoords">The coords of the cell under the cursor.</param>
        public virtual void PostMapDraw(Point2D cellCoords) { }

        /// <summary>
        /// Called when the mouse is moved on the map with the left mouse button down while this action being active.
        /// </summary>
        /// <param name="cellCoords">The coords of the cell under the cursor.</param>
        public virtual void LeftDown(Point2D cellCoords) { }

        /// <summary>
        /// Called when the left mouse button is clicked (pressed and released) on the map with this action being active.
        /// </summary>
        /// <param name="cellCoords">The coords of the cell under the cursor.</param>
        public virtual void LeftClick(Point2D cellCoords) { }

        /// <summary>
        /// Called after drawing the map.
        /// Override in derived classes to draw on top of the map texture.
        /// </summary>
        /// <param name="cellCoords">The coords of the cell under the cursor.</param>
        /// <param name="cameraTopLeftPoint">The top-left point of the user's screen.</param>
        public virtual void DrawPreview(Point2D cellCoords, Point2D cameraTopLeftPoint) { }
    }
}
