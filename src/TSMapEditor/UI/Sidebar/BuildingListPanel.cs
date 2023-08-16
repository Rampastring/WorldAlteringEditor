using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.Sidebar
{
    public class BuildingListPanel : ObjectListPanel
    {
        public BuildingListPanel(WindowManager windowManager, EditorState editorState,
            Map map, TheaterGraphics theaterGraphics, ICursorActionTarget cursorActionTarget) : 
            base(windowManager, editorState, map, theaterGraphics)
        {
            buildingPlacementAction = new BuildingPlacementAction(cursorActionTarget, Keyboard);
            buildingPlacementAction.ActionExited += BuildingPlacementAction_ActionExited;
        }

        private void BuildingPlacementAction_ActionExited(object sender, System.EventArgs e)
        {
            ObjectTreeView.SelectedNode = null;
        }

        private readonly BuildingPlacementAction buildingPlacementAction;

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.BuildingTypes, TheaterGraphics.BuildingTextures);
        }

        protected override void ObjectSelected()
        {
            if (ObjectTreeView.SelectedNode == null)
                return;

            buildingPlacementAction.BuildingType = (BuildingType)ObjectTreeView.SelectedNode.Tag;
            EditorState.CursorAction = buildingPlacementAction;
        }
    }
}
