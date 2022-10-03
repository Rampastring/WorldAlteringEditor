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

        public bool AllowComma { get; set; } = true;

        protected override bool AllowCharacterInput(char character)
        {
            if (character == ',')
                return AllowComma;

            return base.AllowCharacterInput(character);
        }
    }
}
