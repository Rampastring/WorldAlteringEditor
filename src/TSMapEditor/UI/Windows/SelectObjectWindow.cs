using System;
using Rampastring.XNAUI;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
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

        private SelectObjectWindowInfoPanel infoPanel;

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
            lbObjectList.HoveredIndexChanged += LbObjectList_HoveredIndexChanged;

            FindChild<EditorButton>("btnSelect").LeftClick += (s, e) => Hide();

            tbSearch.TextChanged += TbSearch_TextChanged;
            tbSearch.EnterPressed += (s, e) => FindNext();

            // Make pressing X not save changes
            if (btnClose != null)
                btnClose.LeftClick += (s, e) => SelectedObject = initialSelection;

            infoPanel = new SelectObjectWindowInfoPanel(WindowManager);
            AddChild(infoPanel);

            EnabledChanged += SelectObjectWindow_EnabledChanged;
        }

        private void TbSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearch.Text) || tbSearch.Text == tbSearch.Suggestion)
                return;

            lbObjectList.SelectedIndex = -1;
            FindNext();
        }

        private void SelectObjectWindow_EnabledChanged(object sender, EventArgs e)
        {
            if (!Enabled)
            {
                HideInfoPanel();
            }
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

        private void HideInfoPanel()
        {
            if (infoPanel.Detached)
            {
                infoPanel.Attach();
                infoPanel.Disable();
            }
        }

        public void Open(T initialSelection)
        {
            HideInfoPanel();
            SelectedObject = initialSelection;
            this.initialSelection = SelectedObject;
            ListObjects();

            if (lbObjectList.SelectedItem == null)
            {
                // If the initially selected object wasn't found for some reason, then clear selection
                SelectedObject = default;
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

        private void LbObjectList_HoveredIndexChanged(object sender, EventArgs e)
        {
            if (lbObjectList.HoveredItem == null)
            {
                 infoPanel.Hide();
                 return;
            }

            if (lbObjectList.HoveredItem.Tag == null)
                return;

            var item = (T)lbObjectList.HoveredItem.Tag;

            if (item is not IHintable)
                return;

            var itemAsHintable = item as IHintable;

            infoPanel.Open(itemAsHintable.GetHeaderText(),
                itemAsHintable.GetHintText(),
                new Point2D(Width, GetCursorPoint().Y));
        }

        protected abstract void ListObjects();
    }
}
