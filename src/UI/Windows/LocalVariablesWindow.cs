using System;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class LocalVariablesWindow : INItializableWindow
    {
        public LocalVariablesWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorListBox lbLocalVariables;
        private EditorTextBox tbName;
        private XNACheckBox chkInitialState;

        private LocalVariable editedLocalVariable;

        public override void Initialize()
        {
            Name = nameof(LocalVariablesWindow);
            base.Initialize();

            lbLocalVariables = FindChild<EditorListBox>(nameof(lbLocalVariables));
            tbName = FindChild<EditorTextBox>(nameof(tbName));
            chkInitialState = FindChild<XNACheckBox>(nameof(chkInitialState));

            FindChild<EditorButton>("btnNewLocalVariable").LeftClick += BtnNewLocalVariable_LeftClick;

            lbLocalVariables.SelectedIndexChanged += LbLocalVariables_SelectedIndexChanged;
        }

        private void BtnNewLocalVariable_LeftClick(object sender, EventArgs e)
        {
            map.LocalVariables.Add(new LocalVariable(map.LocalVariables.Count) { Name = "New Local Variable" });
            ListLocalVariables();
            lbLocalVariables.SelectedIndex = map.LocalVariables.Count - 1;
            lbLocalVariables.ScrollToBottom();
        }

        private void LbLocalVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbName.TextChanged -= TbName_TextChanged;
            chkInitialState.CheckedChanged -= ChkInitialState_CheckedChanged;

            if (lbLocalVariables.SelectedItem == null)
            {
                editedLocalVariable = null;
                tbName.Text = string.Empty;
                return;
            }

            editedLocalVariable = (LocalVariable)lbLocalVariables.SelectedItem.Tag;
            tbName.Text = editedLocalVariable.Name;
            chkInitialState.Checked = editedLocalVariable.InitialState;

            tbName.TextChanged += TbName_TextChanged;
            chkInitialState.CheckedChanged += ChkInitialState_CheckedChanged;
        }

        private void ChkInitialState_CheckedChanged(object sender, EventArgs e)
        {
            editedLocalVariable.InitialState = chkInitialState.Checked;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            editedLocalVariable.Name = tbName.Text;
            ListLocalVariables();
        }

        public void Open()
        {
            Show();
            ListLocalVariables();
        }

        private void ListLocalVariables()
        {
            lbLocalVariables.Clear();

            foreach (var localVariable in map.LocalVariables)
            {
                lbLocalVariables.AddItem(new XNAListBoxItem() { Text = localVariable.Index + " " + localVariable.Name, Tag = localVariable });
            }
        }
    }
}
