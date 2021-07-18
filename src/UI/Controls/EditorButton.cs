using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorButton : XNAButton
    {
        public EditorButton(WindowManager windowManager) : base(windowManager)
        {
            FontIndex = Constants.UIBoldFont;
            Height = Constants.UITextBoxHeight;
        }
    }
}
