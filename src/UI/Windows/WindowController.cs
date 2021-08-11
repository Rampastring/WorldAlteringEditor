using Rampastring.XNAUI.XNAControls;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class WindowController
    {
        private List<EditorWindow> Windows { get; } = new List<EditorWindow>();

        public TaskforcesWindow TaskForcesWindow { get; private set; }
        public ScriptsWindow ScriptsWindow { get; private set; }
        public TeamTypesWindow TeamTypesWindow { get; private set; }
        public TriggersWindow TriggersWindow { get; private set; }
        public PlaceWaypointWindow PlaceWaypointWindow { get; private set; }
        public LocalVariablesWindow LocalVariablesWindow { get; private set; }

        public void Initialize(XNAControl windowParentControl, Map map, EditorState editorState, ICursorActionTarget cursorActionTarget)
        {
            TaskForcesWindow = new TaskforcesWindow(windowParentControl.WindowManager, map);
            Windows.Add(TaskForcesWindow);

            ScriptsWindow = new ScriptsWindow(windowParentControl.WindowManager, map);
            Windows.Add(ScriptsWindow);

            TeamTypesWindow = new TeamTypesWindow(windowParentControl.WindowManager, map);
            Windows.Add(TeamTypesWindow);

            TriggersWindow = new TriggersWindow(windowParentControl.WindowManager, map, editorState, cursorActionTarget);
            Windows.Add(TriggersWindow);

            PlaceWaypointWindow = new PlaceWaypointWindow(windowParentControl.WindowManager, map, cursorActionTarget.MutationManager, cursorActionTarget.MutationTarget);
            Windows.Add(PlaceWaypointWindow);

            LocalVariablesWindow = new LocalVariablesWindow(windowParentControl.WindowManager, map);
            Windows.Add(LocalVariablesWindow);

            foreach (var window in Windows)
            {
                windowParentControl.AddChild(window);
                window.Disable();
                window.CenterOnParent();
            }
        }
    }
}
