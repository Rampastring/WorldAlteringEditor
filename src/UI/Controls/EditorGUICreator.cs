using Rampastring.XNAUI;

namespace TSMapEditor.UI.Controls
{
    public class EditorGUICreator : GUICreator
    {
        private EditorGUICreator()
        {
            AddControl(typeof(EditorButton));
            AddControl(typeof(EditorTextBox));
            AddControl(typeof(EditorNumberTextBox));
            AddControl(typeof(EditorSuggestionTextBox));
        }

        private static EditorGUICreator _instance;
        public static EditorGUICreator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EditorGUICreator();

                return _instance;
            }
        }
    }
}
