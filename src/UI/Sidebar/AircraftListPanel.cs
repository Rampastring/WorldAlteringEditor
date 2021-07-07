using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class AircraftListPanel : ObjectListPanel
    {
        public AircraftListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager, editorState, map, theaterGraphics)
        {
        }

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.AircraftTypes, null);
        }
    }
}
