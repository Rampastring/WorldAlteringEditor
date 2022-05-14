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

            var customUISettings = UISettings.ActiveSettings as CustomUISettings;

            IdleTexture = Helpers.CreateUITexture(GraphicsDevice, Width, Height,
                customUISettings.ButtonMainBackgroundColor,
                customUISettings.ButtonSecondaryBackgroundColor,
                customUISettings.ButtonTertiaryBackgroundColor);

            HoverTexture = Helpers.CreateUITexture(GraphicsDevice, Width, Height,
                new Color(128, 128, 128, 196),
                new Color(128, 128, 128, 255), Color.White);
        }
    }
}
