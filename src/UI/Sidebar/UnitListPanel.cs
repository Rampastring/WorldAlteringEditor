using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A sidebar panel for listing units.
    /// </summary>
    public class UnitListPanel : ObjectListPanel
    {
        public UnitListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager, editorState, map, theaterGraphics)
        {
        }

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.UnitTypes, TheaterGraphics.UnitTextures);
        }
    }
}
