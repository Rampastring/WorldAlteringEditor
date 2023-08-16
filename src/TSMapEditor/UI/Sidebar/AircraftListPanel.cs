using System;
using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A sidebar panel for listing aircraft.
    /// </summary>
    public class AircraftListPanel : ObjectListPanel
    {
        public AircraftListPanel(WindowManager windowManager, EditorState editorState,
            Map map, TheaterGraphics theaterGraphics, ICursorActionTarget cursorActionTarget)
            : base(windowManager, editorState, map, theaterGraphics)
        {
            aircraftPlacementAction = new AircraftPlacementAction(cursorActionTarget, Keyboard);
            aircraftPlacementAction.ActionExited += AircraftPlacementAction_ActionExited;
        }

        private void AircraftPlacementAction_ActionExited(object sender, EventArgs e)
        {
            ObjectTreeView.SelectedNode = null;
        }

        private readonly AircraftPlacementAction aircraftPlacementAction;

        protected override void InitObjects()
        {
            InitObjectsBase(Map.Rules.AircraftTypes, null);
        }

        protected override void ObjectSelected()
        {
            if (ObjectTreeView.SelectedNode == null)
                return;

            aircraftPlacementAction.AircraftType = (AircraftType)ObjectTreeView.SelectedNode.Tag;
            EditorState.CursorAction = aircraftPlacementAction;
        }
    }
}
