using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A base class for all object type list panels.
    /// </summary>
    public abstract class ObjectListPanel : XNAPanel
    {
        public ObjectListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            EditorState = editorState;
            Map = map;
            TheaterGraphics = theaterGraphics;
        }

        protected EditorState EditorState { get; }
        protected Map Map { get; }
        protected TheaterGraphics TheaterGraphics { get; }

        public XNASuggestionTextBox SearchBox { get; private set; }
        public TreeView ObjectTreeView { get; private set; }

        private XNADropDown ddOwner;

        public override void Initialize()
        {
            var lblOwner = new XNALabel(WindowManager);
            lblOwner.Name = nameof(lblOwner);
            lblOwner.X = Constants.UIEmptySideSpace;
            lblOwner.Y = Constants.UIEmptyTopSpace;
            lblOwner.Text = "Owner:";
            AddChild(lblOwner);

            ddOwner = new XNADropDown(WindowManager);
            ddOwner.Name = nameof(ddOwner);
            ddOwner.X = lblOwner.Right + Constants.UIHorizontalSpacing;
            ddOwner.Y = lblOwner.Y - 1;
            ddOwner.Width = Width - Constants.UIEmptySideSpace - ddOwner.X;
            AddChild(ddOwner);
            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;

            SearchBox = new XNASuggestionTextBox(WindowManager);
            SearchBox.Name = nameof(SearchBox);
            SearchBox.X = Constants.UIEmptySideSpace;
            SearchBox.Y = ddOwner.Bottom + Constants.UIEmptyTopSpace;
            SearchBox.Width = Width - Constants.UIEmptySideSpace * 2;
            SearchBox.Height = Constants.UITextBoxHeight;
            SearchBox.Suggestion = "Search object... (CTRL + F)";
            AddChild(SearchBox);
            SearchBox.TextChanged += SearchBox_TextChanged;
            SearchBox.EnterPressed += SearchBox_EnterPressed;

            ObjectTreeView = new TreeView(WindowManager);
            ObjectTreeView.Name = nameof(ObjectTreeView);
            ObjectTreeView.Y = SearchBox.Bottom + Constants.UIVerticalSpacing;
            ObjectTreeView.Height = Height - ObjectTreeView.Y;
            ObjectTreeView.Width = Width;
            AddChild(ObjectTreeView);
            ObjectTreeView.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 222), 2, 2);

            EditorState.ObjectOwnerChanged += EditorState_ObjectOwnerChanged;

            base.Initialize();

            RefreshHouseList();

            Parent.ClientRectangleUpdated += Parent_ClientRectangleUpdated;
            RefreshPanelSize();

            InitObjects();
        }

        private void SearchBox_EnterPressed(object sender, EventArgs e)
        {
            ObjectTreeView.FindNode(SearchBox.Text, true);
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == SearchBox.Suggestion)
                return;

            ObjectTreeView.FindNode(SearchBox.Text, false);
        }

        protected abstract void InitObjects();

        private void Parent_ClientRectangleUpdated(object sender, EventArgs e)
        {
            RefreshSize();
        }

        private void RefreshPanelSize()
        {
            Width = Parent.Width;
            ddOwner.Width = Width - Constants.UIEmptySideSpace - ddOwner.X;
            SearchBox.Width = Width - Constants.UIEmptySideSpace * 2;
            ObjectTreeView.Width = Width;
        }

        private void DdOwner_SelectedIndexChanged(object sender, EventArgs e)
        {
            EditorState.ObjectOwner = Map.Houses[ddOwner.SelectedIndex];
        }

        private void RefreshHouseList()
        {
            ddOwner.SelectedIndexChanged -= DdOwner_SelectedIndexChanged;

            ddOwner.Items.Clear();
            Map.Houses.ForEach(h => ddOwner.AddItem(h.ININame, h.XNAColor));
            ddOwner.SelectedIndex = Map.Houses.FindIndex(h => h == EditorState.ObjectOwner);

            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;
        }

        private void EditorState_ObjectOwnerChanged(object sender, EventArgs e)
        {
            ddOwner.SelectedIndexChanged -= DdOwner_SelectedIndexChanged;

            ddOwner.SelectedIndex = Map.Houses.FindIndex(h => h == EditorState.ObjectOwner);

            ddOwner.SelectedIndexChanged += DdOwner_SelectedIndexChanged;
        }
    }
}
