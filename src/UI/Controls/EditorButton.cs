using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorButton : XNAButton
    {
        public EditorButton(WindowManager windowManager) : base(windowManager)
        {
            FontIndex = Constants.UIBoldFont;
            Height = Constants.UIButtonHeight;
        }

        public override void Initialize()
        {
            base.Initialize();

            IdleTexture = Helpers.CreateUITexture(GraphicsDevice, Width, Height,
                new Color(0, 0, 0, 196), new Color(0, 0, 0, 255), Color.White);

            HoverTexture = Helpers.CreateUITexture(GraphicsDevice, Width, Height,
                new Color(128, 128, 128, 196),
                new Color(128, 128, 128, 255), Color.White);
        }
    }
}
