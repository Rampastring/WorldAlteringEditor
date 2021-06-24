using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    public abstract class CursorAction
    {
        /// <summary>
        /// Called prior to drawing the map.
        /// </summary>
        public virtual void PreMapDraw(Point2D cellCoordsOnCursor, ICursorActionTarget cursorActionTarget) { }

        /// <summary>
        /// Called after drawing the map.
        /// Override in derived classes to clear preview data related to the action.
        /// </summary>
        public virtual void PostMapDraw(Point2D cellCoordsOnCursor, ICursorActionTarget cursorActionTarget) { }

        public virtual void LeftDown(Point2D cellPoint, ICursorActionTarget cursorActionTarget) { }

        public abstract void LeftClick(Point2D cellPoint, ICursorActionTarget cursorActionTarget);

        public abstract void DrawPreview(Point2D cellTopLeftPoint, ICursorActionTarget cursorActionTarget);
    }
}
