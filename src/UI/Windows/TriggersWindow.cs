using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class TriggersWindow : INItializableWindow
    {
        public TriggersWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        // Trigger list
        private EditorListBox lbTriggers;

        // General trigger settings
        private EditorTextBox tbName;
        private XNADropDown ddHouse;
        private XNADropDown ddType;
        private EditorPopUpSelector selAttachedTrigger;
        private XNACheckBox chkDisabled;
        private XNACheckBox chkEasy;
        private XNACheckBox chkMedium;
        private XNACheckBox chkHard;

        // Events
        private EditorListBox lbEvents;
        private EditorPopUpSelector selEventType;
        private EditorDescriptionPanel panelEventDescription;
        private EditorListBox lbEventParameters;
        private EditorNumberTextBox tbEventParameterValue;

        // Actions
        private EditorListBox lbActions;
        private EditorPopUpSelector selActionType;
        private EditorDescriptionPanel panelActionDescription;
        private EditorListBox lbActionParameters;
        private EditorNumberTextBox tbActionParameterValue;

        private Trigger editedTrigger;

        public override void Initialize()
        {
            Name = nameof(TriggersWindow);
            base.Initialize();

            lbTriggers = FindChild<EditorListBox>(nameof(lbTriggers));
            tbName = FindChild<EditorTextBox>(nameof(tbName));
            ddHouse = FindChild<XNADropDown>(nameof(ddHouse));
            ddType = FindChild<XNADropDown>(nameof(ddType));
            selAttachedTrigger = FindChild<EditorPopUpSelector>(nameof(selAttachedTrigger));
            chkDisabled = FindChild<XNACheckBox>(nameof(chkDisabled));
            chkEasy = FindChild<XNACheckBox>(nameof(chkEasy));
            chkMedium = FindChild<XNACheckBox>(nameof(chkMedium));
            chkHard = FindChild<XNACheckBox>(nameof(chkHard));

            lbEvents = FindChild<EditorListBox>(nameof(lbEvents));
            selEventType = FindChild<EditorPopUpSelector>(nameof(selEventType));
            panelEventDescription = FindChild<EditorDescriptionPanel>(nameof(panelEventDescription));
            lbEventParameters = FindChild<EditorListBox>(nameof(lbEventParameters));
            tbEventParameterValue = FindChild<EditorNumberTextBox>(nameof(tbEventParameterValue));

            lbActions = FindChild<EditorListBox>(nameof(lbActions));
            selActionType = FindChild<EditorPopUpSelector>(nameof(selActionType));
            panelActionDescription = FindChild<EditorDescriptionPanel>(nameof(panelActionDescription));
            lbActionParameters = FindChild<EditorListBox>(nameof(lbActionParameters));
            tbActionParameterValue = FindChild<EditorNumberTextBox>(nameof(tbActionParameterValue));

            lbTriggers.SelectedIndexChanged += LbTriggers_SelectedIndexChanged;
        }

        private void LbTriggers_SelectedIndexChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            ListTriggers();
            Show();
        }

        private void ListTriggers()
        {
            lbTriggers.Clear();

            foreach (Trigger trigger in map.Triggers)
            {
                lbTriggers.AddItem(new XNAListBoxItem() { Text = trigger.Name, Tag = trigger });
            }
        }
    }
}
