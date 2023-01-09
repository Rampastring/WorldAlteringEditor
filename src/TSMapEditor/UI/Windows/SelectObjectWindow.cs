using System;
using Rampastring.XNAUI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public abstract class SelectObjectWindow<T> : INItializableWindow
    {
        public SelectObjectWindow(WindowManager windowManager) : base(windowManager)
        {
            HasCloseButton = true;
        }

        protected EditorSuggestionTextBox tbSearch;
        protected EditorListBox lbObjectList;

        /// <summary>
        /// If the object is being selected for a trigger event or action,
        /// this can be used to determine whether the object is selected
        /// for an event or an action.
        /// </summary>
        public bool IsForEvent { get; set; }

        public T SelectedObject { get; protected set; }

        private T initialSelection;

        public override void Initialize()
        {
            base.Initialize();

            DrawOrder = WindowController.ChildWindowOrderValue * 2;
            UpdateOrder = DrawOrder;

            if (Parent != null)
            {
                Parent.DrawOrder = DrawOrder;
                Parent.UpdateOrder = UpdateOrder;
            }

            tbSearch = FindChild<EditorSuggestionTextBox>(nameof(tbSearch));
            UIHelpers.AddSearchTipsBoxToControl(tbSearch);

            lbObjectList = FindChild<EditorListBox>(nameof(lbObjectList));

            lbObjectList.AllowRightClickUnselect = false;
            lbObjectList.DoubleLeftClick += (s, e) => Hide();
            lbObjectList.SelectedIndexChanged += LbObjectList_SelectedIndexChanged;

            FindChild<EditorButton>("btnSelect").LeftClick += (s, e) => Hide();

            tbSearch.TextChanged += TbSearch_TextChanged;
            tbSearch.EnterPressed += (s, e) => FindNext();

            // Make pressing X not save changes
            if (btnClose != null)
                btnClose.LeftClick += (s, e) => SelectedObject = initialSelection;
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
                    lbObjectList.ViewTop = lbObjectList.SelectedIndex * lbObjectList.LineHeight;
                    break;
                }
            }
        }

        public void Open(T initialSelection)
        {
            SelectedObject = initialSelection;
            initialSelection = SelectedObject;
            ListObjects();

            if (lbObjectList.SelectedItem == null)
            {
                // If the initially selected object wasn't found for some reason, then clear selection
                SelectedObject = default(T);
            }

            if (SelectedObject == null)
            {
                lbObjectList.SelectedIndex = -1;
            }

            if (lbObjectList.SelectedIndex > lbObjectList.LastIndex)
            {
                lbObjectList.LastIndex = lbObjectList.SelectedIndex;
            }

            Show();
            WindowManager.SelectedControl = tbSearch;
        }

        protected abstract void LbObjectList_SelectedIndexChanged(object sender, EventArgs e);

        protected abstract void ListObjects();
    }
}
