using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class ScriptsWindow : INItializableWindow
    {
        public ScriptsWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorListBox lbScriptTypes;
        private EditorTextBox tbName;
        private EditorListBox lbActions;
        private EditorPopUpSelector selTypeOfAction;
        private XNALabel lblParameterDescription;
        private EditorNumberTextBox tbParameterValue;
        private MenuButton btnEditorPresetValues;
        private XNALabel lblActionDescriptionValue;

        private Script editedScript;

        private SelectScriptActionWindow selectScriptActionWindow;

        public override void Initialize()
        {
            Name = nameof(ScriptsWindow);
            base.Initialize();

            lbScriptTypes = FindChild<EditorListBox>(nameof(lbScriptTypes));
            tbName = FindChild<EditorTextBox>(nameof(tbName));
            lbActions = FindChild<EditorListBox>(nameof(lbActions));
            selTypeOfAction = FindChild<EditorPopUpSelector>(nameof(selTypeOfAction));
            lblParameterDescription = FindChild<XNALabel>(nameof(lblParameterDescription));
            tbParameterValue = FindChild<EditorNumberTextBox>(nameof(tbParameterValue));
            btnEditorPresetValues = FindChild<MenuButton>(nameof(btnEditorPresetValues));
            lblActionDescriptionValue = FindChild<XNALabel>(nameof(lblActionDescriptionValue));

            var btnAddScript = FindChild<EditorButton>("btnAddScript");
            var btnDeleteScript = FindChild<EditorButton>("btnDeleteScript");
            var btnCloneScript = FindChild<EditorButton>("btnCloneScript");

            var btnAddAction = FindChild<EditorButton>("btnAddAction");
            var btnDeleteAction = FindChild<EditorButton>("btnDeleteAction");

            lbScriptTypes.SelectedIndexChanged += LbScriptTypes_SelectedIndexChanged;
            lbActions.SelectedIndexChanged += LbActions_SelectedIndexChanged;

            selectScriptActionWindow = new SelectScriptActionWindow(WindowManager, map.EditorConfig);
            var darkeningPanel = new DarkeningPanel(WindowManager);
            darkeningPanel.DrawOrder = 1;
            darkeningPanel.UpdateOrder = 1;
            Parent.AddChild(darkeningPanel);
            darkeningPanel.AddChild(selectScriptActionWindow);
            darkeningPanel.Hide();
            darkeningPanel.Alpha = 0f;
            darkeningPanel.Hidden += DarkeningPanel_Hidden;
            selectScriptActionWindow.CenterOnParent();

            selTypeOfAction.MouseLeftDown += SelTypeOfAction_MouseLeftDown; ;
        }

        private void SelTypeOfAction_MouseLeftDown(object sender, EventArgs e)
        {
            if (lbActions.SelectedItem == null || editedScript == null)
            {
                return;
            }

            ScriptActionEntry entry = editedScript.Actions[lbActions.SelectedIndex];
            selectScriptActionWindow.Open(entry.Action);
        }

        private void DarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (lbActions.SelectedItem == null || editedScript == null)
            {
                return;
            }

            ScriptActionEntry entry = editedScript.Actions[lbActions.SelectedIndex];
            entry.Action = selectScriptActionWindow.SelectedActionIndex;
            lbActions.Items[lbActions.SelectedIndex].Text = GetActionEntryText(lbActions.SelectedIndex, entry);
            LbActions_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void LbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbActions.SelectedItem == null || editedScript == null)
            {
                selTypeOfAction.Text = string.Empty;
                selTypeOfAction.Tag = null;
                tbParameterValue.Text = string.Empty;
                lblActionDescriptionValue.Text = string.Empty;
                return;
            }

            ScriptActionEntry entry = editedScript.Actions[lbActions.SelectedIndex];
            ScriptAction action = entry.Action >= map.EditorConfig.ScriptActions.Count ? null : map.EditorConfig.ScriptActions[entry.Action];

            selTypeOfAction.Text = GetActionNameFromIndex(entry.Action);
            tbParameterValue.Value = entry.Argument;
            lblParameterDescription.Text = action == null ? "Parameter:" : action.ParamDescription + ":";
            lblActionDescriptionValue.Text = GetActionDescriptionFromIndex(entry.Action);
        }

        private void LbScriptTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbScriptTypes.SelectedItem == null)
            {
                EditScript(null);
                return;
            }

            EditScript((Script)lbScriptTypes.SelectedItem.Tag);
        }

        public void Open()
        {
            ListScripts();

            Show();
        }

        private void ListScripts()
        {
            lbScriptTypes.Clear();

            foreach (var script in map.Scripts)
            {
                lbScriptTypes.AddItem(new XNAListBoxItem() { Text = script.Name, Tag = script });
            }
        }

        private void EditScript(Script script)
        {
            editedScript = script;

            lbActions.Clear();

            if (editedScript == null)
            {
                tbName.Text = string.Empty;
                selTypeOfAction.Text = string.Empty;
                selTypeOfAction.Tag = null;
                tbParameterValue.Text = string.Empty;
                btnEditorPresetValues.ContextMenu.ClearItems();
                lblActionDescriptionValue.Text = string.Empty;

                return;
            }

            tbName.Text = editedScript.Name;
            for (int i = 0; i < editedScript.Actions.Count; i++)
            {
                var actionEntry = editedScript.Actions[i];
                lbActions.AddItem(new XNAListBoxItem() 
                { 
                    Text = GetActionEntryText(i, actionEntry), 
                    Tag = actionEntry
                });
            }

            LbActions_SelectedIndexChanged(this, EventArgs.Empty);
        }

        private string GetActionEntryText(int index, ScriptActionEntry entry)
        {
            ScriptAction action = null;
            if (entry.Action >= map.EditorConfig.ScriptActions.Count)
                return index + " - Unknown";

            action = map.EditorConfig.ScriptActions[entry.Action];
            return index + " - " + action.Name;
        }

        private string GetActionNameFromIndex(int index)
        {
            if (index >= map.EditorConfig.ScriptActions.Count)
                return index + " Unknown";

            var action = map.EditorConfig.ScriptActions[index];
            return index + " " + action.Name;
        }

        private string GetActionDescriptionFromIndex(int index)
        {
            if (index >= map.EditorConfig.ScriptActions.Count)
                return string.Empty;

            var action = map.EditorConfig.ScriptActions[index];

            return Renderer.FixText(action.Description,
                lblActionDescriptionValue.FontIndex,
                lblActionDescriptionValue.Parent.Width - lblActionDescriptionValue.X * 2).Text;
        }
    }
}
