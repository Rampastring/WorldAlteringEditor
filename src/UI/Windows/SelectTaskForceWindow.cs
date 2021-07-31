using System;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to select a TaskForce (for example, for a TeamType).
    /// </summary>
    public class SelectTaskForceWindow : INItializableWindow
    {
        public SelectTaskForceWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorSuggestionTextBox tbSearch;
        private EditorListBox lbTaskForces;

        public TaskForce SelectedTaskForce { get; private set; }

        public override void Initialize()
        {
            Name = nameof(SelectTaskForceWindow);
            base.Initialize();

            tbSearch = FindChild<EditorSuggestionTextBox>(nameof(tbSearch));
            lbTaskForces = FindChild<EditorListBox>(nameof(lbTaskForces));

            lbTaskForces.AllowRightClickUnselect = false;
            lbTaskForces.DoubleLeftClick += LbScriptActions_DoubleLeftClick;
            lbTaskForces.SelectedIndexChanged += LbTaskForces_SelectedIndexChanged;
            FindChild<EditorButton>("btnSelect").LeftClick += BtnSelect_LeftClick;
        }

        private void LbTaskForces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTaskForces.SelectedItem == null)
            {
                SelectedTaskForce = null;
                return;
            }

            SelectedTaskForce = (TaskForce)lbTaskForces.SelectedItem.Tag;
        }

        private void LbScriptActions_DoubleLeftClick(object sender, EventArgs e)
        {
            Hide();
        }

        private void BtnSelect_LeftClick(object sender, EventArgs e)
        {
            Hide();
        }

        public void Open(TaskForce initialSelection)
        {
            SelectedTaskForce = initialSelection;
            ListTaskForces();
            Show();
            Alpha = 1.0f;
        }

        private void ListTaskForces()
        {
            lbTaskForces.Clear();

            foreach (TaskForce taskForce in map.TaskForces)
            {
                lbTaskForces.AddItem(new XNAListBoxItem() { Text = $"{taskForce.Name} ({taskForce.ININame})", Tag = taskForce });
                if (taskForce == SelectedTaskForce)
                    lbTaskForces.SelectedIndex = lbTaskForces.Items.Count - 1;
            }

            // If the initial selection taskforce wasn't found for some reason, then clear selection
            if (lbTaskForces.SelectedItem == null)
                SelectedTaskForce = null;
        }
    }
}
