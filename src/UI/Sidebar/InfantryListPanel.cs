using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class InfantryListPanel : ObjectListPanel
    {
        public InfantryListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager, editorState, map, theaterGraphics)
        {
        }

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.InfantryTypes, TheaterGraphics.InfantryTextures);
        }
    }
}
