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

        private bool isStockBackgroundTexture;

        public override void Initialize()
        {
            base.Initialize();

            if (BackgroundTexture == null)
            {
                BackgroundTexture = AssetLoader.CreateTexture(UISettings.ActiveSettings.PanelBackgroundColor, 2, 2);
                PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
                isStockBackgroundTexture = true;
            }
        }

        public override void Kill()
        {
            if (isStockBackgroundTexture)
                BackgroundTexture?.Dispose();

            base.Kill();
        }
    }
}
