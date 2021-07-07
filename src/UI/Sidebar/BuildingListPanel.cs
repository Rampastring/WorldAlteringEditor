using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class BuildingListPanel : ObjectListPanel
    {
        public BuildingListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager, editorState, map, theaterGraphics)
        {
        }

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.BuildingTypes, TheaterGraphics.BuildingTextures);
        }

        protected override void ObjectSelected()
        {
            throw new System.NotImplementedException();
        }
    }
}
