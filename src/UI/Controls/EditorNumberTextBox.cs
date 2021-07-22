using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorNumberTextBox : EditorTextBox
    {
        public EditorNumberTextBox(WindowManager windowManager) : base(windowManager)
        {
        }

        protected override bool AllowCharacterInput(char character)
        {
            return character >= '0' && character <= '9';
        }
    }
}
