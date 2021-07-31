using System;
using Rampastring.XNAUI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public abstract class SelectObjectWindow<T> : INItializableWindow
    {
        public SelectObjectWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        protected EditorSuggestionTextBox tbSearch;
        protected EditorListBox lbObjectList;

        public T SelectedObject { get; protected set; }

        public override void Initialize()
        {
            Name = nameof(SelectTaskForceWindow);
            base.Initialize();

            tbSearch = FindChild<EditorSuggestionTextBox>(nameof(tbSearch));
            lbObjectList = FindChild<EditorListBox>(nameof(lbObjectList));

            lbObjectList.AllowRightClickUnselect = false;
            lbObjectList.DoubleLeftClick += (s, e) => Hide();
            lbObjectList.SelectedIndexChanged += LbObjectList_SelectedIndexChanged;

            FindChild<EditorButton>("btnSelect").LeftClick += (s, e) => Hide();

            tbSearch.TextChanged += TbSearch_TextChanged;
            tbSearch.EnterPressed += (s, e) => FindNext();
        }

        private void TbSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearch.Text) || tbSearch.Text == tbSearch.Suggestion)
                return;

            lbObjectList.SelectedIndex = -1;
            FindNext();
        }

        private void FindNext()
        {
            for (int i = lbObjectList.SelectedIndex + 1; i < lbObjectList.Items.Count; i++)
            {
                if (lbObjectList.Items[i].Text.ToUpperInvariant().Contains(tbSearch.Text.ToUpperInvariant()))
                {
                    lbObjectList.SelectedIndex = i;
                    break;
                }
            }
        }

        public void Open(T initialSelection)
        {
            SelectedObject = initialSelection;
            ListObjects();
            Show();
        }

        protected abstract void LbObjectList_SelectedIndexChanged(object sender, EventArgs e);

        protected abstract void ListObjects();
    }
}
