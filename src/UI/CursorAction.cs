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
        public abstract void PerformAction(Point2D cellPoint, ICursorActionTarget cursorActionTarget);

        public abstract void DrawPreview(Point2D cellTopLeftPoint, ICursorActionTarget cursorActionTarget);
    }
}
