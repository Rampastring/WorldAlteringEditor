using Rampastring.XNAUI;

namespace TSMapEditor.UI.Controls
{
    public class EditorGUICreator : GUICreator
    {
        private EditorGUICreator()
        {
            AddControl(typeof(EditorPanel));
            AddControl(typeof(EditorButton));
            AddControl(typeof(MenuButton));
            AddControl(typeof(EditorListBox));
            AddControl(typeof(EditorTextBox));
            AddControl(typeof(EditorNumberTextBox));
            AddControl(typeof(EditorSuggestionTextBox));
            AddControl(typeof(EditorPopUpSelector));
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
