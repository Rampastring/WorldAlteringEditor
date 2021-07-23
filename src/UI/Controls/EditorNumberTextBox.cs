using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System.Globalization;

namespace TSMapEditor.UI.Controls
{
    public class EditorNumberTextBox : EditorTextBox
    {
        public EditorNumberTextBox(WindowManager windowManager) : base(windowManager)
        {
        }

        public int DefaultValue { get; set; } = 0;

        protected override bool AllowCharacterInput(char character)
        {
            return (character >= '0' && character <= '9') || character == '-';
        }

        public int Value
        {
            get => Conversions.IntFromString(Text, 0);
            set => Text = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
