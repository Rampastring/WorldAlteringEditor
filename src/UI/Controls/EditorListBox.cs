using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorListBox : XNAListBox
    {
        public EditorListBox(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (BackgroundTexture == null)
            {
                BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 196), 2, 2);
                PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            }
        }
    }
}
