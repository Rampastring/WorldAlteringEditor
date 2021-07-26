using Rampastring.XNAUI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class TeamTypesWindow : INItializableWindow
    {
        public TeamTypesWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            Name = nameof(TeamTypesWindow);
            base.Initialize();
        }

        public void Open()
        {
            Show();
        }
    }
}
