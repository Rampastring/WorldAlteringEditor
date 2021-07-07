using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A sidebar panel for listing units.
    /// </summary>
    public class UnitListPanel : ObjectListPanel
    {
        public UnitListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics, ICursorActionTarget cursorActionTarget) : base(windowManager, editorState, map, theaterGraphics)
        {
            unitPlacementAction = new UnitPlacementAction(cursorActionTarget);
            unitPlacementAction.ActionExited += UnitPlacementAction_ActionExited;
        }

        private void UnitPlacementAction_ActionExited(object sender, System.EventArgs e)
        {
            ObjectTreeView.SelectedNode = null;
        }

        private UnitPlacementAction unitPlacementAction;

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.UnitTypes, TheaterGraphics.UnitTextures);
        }

        protected override void ObjectSelected()
        {
            if (ObjectTreeView.SelectedNode == null)
                return;

            unitPlacementAction.UnitType = (UnitType)ObjectTreeView.SelectedNode.Tag;
            EditorState.CursorAction = unitPlacementAction;
        }
    }
}
