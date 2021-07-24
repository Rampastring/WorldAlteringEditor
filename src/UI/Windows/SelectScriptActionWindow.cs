using Rampastring.XNAUI;
using System;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class SelectScriptActionWindow : INItializableWindow
    {
        public SelectScriptActionWindow(WindowManager windowManager, EditorConfig editorConfig) : base(windowManager)
        {
            this.editorConfig = editorConfig;
        }

        private EditorConfig editorConfig;

        private EditorSuggestionTextBox tbSearchAction;
        private EditorListBox lbScriptActions;

        public int SelectedActionIndex 
        {
            get => lbScriptActions.SelectedIndex;
            set => lbScriptActions.SelectedIndex = value;
        }

        public override void Initialize()
        {
            Name = nameof(SelectScriptActionWindow);
            base.Initialize();

            tbSearchAction = FindChild<EditorSuggestionTextBox>(nameof(tbSearchAction));
            lbScriptActions = FindChild<EditorListBox>(nameof(lbScriptActions));

            lbScriptActions.AllowRightClickUnselect = false;
            lbScriptActions.DoubleLeftClick += LbScriptActions_DoubleLeftClick;
            var btnSelect = FindChild<EditorButton>("btnSelect");
            btnSelect.LeftClick += BtnSelect_LeftClick;

            editorConfig.ScriptActions.ForEach(a => lbScriptActions.AddItem(a.Index + " " + a.Name));
        }

        private void LbScriptActions_DoubleLeftClick(object sender, EventArgs e)
        {
            Hide();
        }

        private void BtnSelect_LeftClick(object sender, EventArgs e)
        {
            Hide();
        }

        public void Open(int index)
        {
            SelectedActionIndex = index;
            Show();
            Alpha = 1.0f;
        }
    }
}
