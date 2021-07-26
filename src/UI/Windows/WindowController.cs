using Rampastring.XNAUI.XNAControls;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class WindowController
    {
        private List<EditorWindow> Windows { get; } = new List<EditorWindow>();

        public TaskforcesWindow TaskForcesWindow { get; private set; }
        public ScriptsWindow ScriptsWindow { get; private set; }
        public TeamTypesWindow TeamTypesWindow { get; private set; }

        public void Initialize(XNAControl windowParentControl, Map map)
        {
            TaskForcesWindow = new TaskforcesWindow(windowParentControl.WindowManager, map);
            Windows.Add(TaskForcesWindow);

            ScriptsWindow = new ScriptsWindow(windowParentControl.WindowManager, map);
            Windows.Add(ScriptsWindow);

            TeamTypesWindow = new TeamTypesWindow(windowParentControl.WindowManager);
            Windows.Add(TeamTypesWindow);

            foreach (var window in Windows)
            {
                windowParentControl.AddChild(window);
                window.Disable();
                window.CenterOnParent();
            }
        }
    }
}
