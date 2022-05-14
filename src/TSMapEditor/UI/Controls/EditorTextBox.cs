using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorTextBox : XNATextBox
    {
        public EditorTextBox(WindowManager windowManager) : base(windowManager)
        {
            Height = Constants.UITextBoxHeight;
        }
    }
}
