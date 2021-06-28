using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI
{
    /// <summary>
    /// A XNAPanel derivative that sets its background.
    /// </summary>
    public class EditorPanel : XNAPanel
    {
        public EditorPanel(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            BackgroundTexture = AssetLoader.CreateTexture(UISettings.ActiveSettings.PanelBackgroundColor, 2, 2);
            PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
        }
    }
}
