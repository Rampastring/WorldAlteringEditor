using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class LightingSettingsWindow : INItializableWindow
    {
        public LightingSettingsWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public override void Initialize()
        {
            Name = nameof(LightingSettingsWindow);
            base.Initialize();
        }

        public void Open()
        {
            Show();
        }
    }
}
