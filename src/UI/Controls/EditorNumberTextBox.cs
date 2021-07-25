using Rampastring.Tools;
using Rampastring.XNAUI;
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
            get
            {
                Conversions.IntFromString(Text, DefaultValue);
                int firstNonDigitIndex = -1;
                int i = 0;
                while (i < Text.Length)
                {
                    if (!char.IsDigit(Text[i]) && Text[i] != '-')
                    {
                        firstNonDigitIndex = i;
                        break;
                    }

                    i++;
                }

                if (firstNonDigitIndex == -1)
                    return Conversions.IntFromString(Text, DefaultValue);

                return Conversions.IntFromString(Text.Substring(0, firstNonDigitIndex), DefaultValue);
            }
            set => Text = value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
