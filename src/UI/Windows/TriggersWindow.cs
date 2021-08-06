using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class TriggersWindow : INItializableWindow
    {
        private const int EVENT_PARAM_FIRST = 0;
        private const int EVENT_PARAM_SECOND = 1;

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

            ddType.AddItem("0 - one-time, single-object condition");
            ddType.AddItem("1 - one-time, multi-object condition");
            ddType.AddItem("2 - repeating, single-object condition");

            lbEvents.AllowMultiLineItems = false;
            lbActions.AllowMultiLineItems = false;

            lbTriggers.SelectedIndexChanged += LbTriggers_SelectedIndexChanged;
        }

        private void LbTriggers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTriggers.SelectedItem == null)
            {
                EditTrigger(null);
                return;
            }

            EditTrigger((Trigger)lbTriggers.SelectedItem.Tag);
        }

        public void Open()
        {
            ListTriggers();
            RefreshHouses();
            Show();
        }

        private void RefreshHouses()
        {
            ddHouse.Items.Clear();
            map.GetHouses().ForEach(h => ddHouse.AddItem(h.ININame, h.XNAColor));
        }

        private void ListTriggers()
        {
            lbTriggers.Clear();

            foreach (Trigger trigger in map.Triggers)
            {
                lbTriggers.AddItem(new XNAListBoxItem() { Text = trigger.Name, Tag = trigger });
            }
        }

        private void EditTrigger(Trigger trigger)
        {
            lbEvents.SelectedIndexChanged -= LbEvents_SelectedIndexChanged;
            lbActions.SelectedIndexChanged -= LbActions_SelectedIndexChanged;

            editedTrigger = trigger;

            if (editedTrigger == null)
            {
                tbName.Text = string.Empty;
                ddHouse.SelectedIndex = -1;
                ddType.SelectedIndex = -1;
                selAttachedTrigger.Text = string.Empty;

                lbEvents.Clear();
                selEventType.Text = string.Empty;
                panelEventDescription.Text = string.Empty;
                lbEventParameters.Clear();
                tbEventParameterValue.Text = string.Empty;

                lbActions.Clear();
                selActionType.Text = string.Empty;
                panelActionDescription.Text = string.Empty;
                lbActionParameters.Clear();
                tbActionParameterValue.Text = string.Empty;

                return;
            }

            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);

            tbName.Text = editedTrigger.Name;
            ddHouse.SelectedIndex = map.GetHouses().FindIndex(h => h.ININame == trigger.House);
            ddType.SelectedIndex = tag == null ? -1 : tag.Repeating;
            selAttachedTrigger.Text = editedTrigger.LinkedTrigger == null ? Constants.NoneValue1 : editedTrigger.LinkedTrigger.Name;
            chkDisabled.Checked = editedTrigger.Disabled;
            chkEasy.Checked = editedTrigger.Easy;
            chkMedium.Checked = editedTrigger.Normal;
            chkHard.Checked = editedTrigger.Hard;

            lbEvents.Clear();
            editedTrigger.Conditions.ForEach(c => AddEvent(c));

            lbActions.Clear();
            editedTrigger.Actions.ForEach(a => AddAction(a));

            LbEvents_SelectedIndexChanged(this, EventArgs.Empty);
            LbActions_SelectedIndexChanged(this, EventArgs.Empty);

            lbEvents.SelectedIndexChanged += LbEvents_SelectedIndexChanged;
            lbActions.SelectedIndexChanged += LbActions_SelectedIndexChanged;
        }

        private void LbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbActionParameters.SelectedIndexChanged -= LbActionParameters_SelectedIndexChanged;

            if (lbActions.SelectedItem == null)
            {
                selActionType.Text = string.Empty;
                panelActionDescription.Text = string.Empty;
                lbActionParameters.Clear();
                tbActionParameterValue.Text = string.Empty;
                return;
            }

            var triggerAction = (TriggerAction)lbActions.SelectedItem.Tag;
            TriggerActionType triggerActionType = null;
            if (triggerAction.ActionIndex < map.EditorConfig.TriggerActionTypes.Count)
                triggerActionType = map.EditorConfig.TriggerActionTypes[triggerAction.ActionIndex];

            selActionType.Text = triggerAction.ActionIndex + " " + (triggerActionType == null ? "Unknown" : triggerActionType.Name);
            panelActionDescription.Text = triggerActionType == null ? "Unknown action. It has most likely been added with another editor." : triggerActionType.Description;

            lbActionParameters.Clear();
            if (triggerActionType == null)
            {
                for (int i = 0; i < TriggerActionType.MAX_PARAM_COUNT; i++)
                {
                    lbActionParameters.AddItem(new XNAListBoxItem() { Text = $"Parameter {i}", Tag = i });
                }
            }
            else
            {
                for (int i = 0; i < triggerActionType.Parameters.Length; i++)
                {
                    var param = triggerActionType.Parameters[i];
                    if (param.TriggerParamType == TriggerParamType.Unused || (int)param.TriggerParamType < 0)
                        continue;

                    lbActionParameters.AddItem(new XNAListBoxItem() { Text = param.NameOverride ?? param.TriggerParamType.ToString(), Tag = i });
                }
            }

            LbActionParameters_SelectedIndexChanged(this, EventArgs.Empty);

            lbActionParameters.SelectedIndexChanged += LbActionParameters_SelectedIndexChanged;
        }

        private void LbActionParameters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbActionParameters.SelectedItem == null || editedTrigger == null || lbActions.SelectedItem == null)
            {
                tbActionParameterValue.Text = string.Empty;
                return;
            }

            tbActionParameterValue.Text = editedTrigger.Actions[lbActions.SelectedIndex].Parameters[(int)lbActionParameters.SelectedItem.Tag];
        }

        private void LbEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbEventParameters.SelectedIndexChanged -= LbEventParameters_SelectedIndexChanged;

            if (lbEvents.SelectedItem == null)
            {
                selEventType.Text = string.Empty;
                panelEventDescription.Text = string.Empty;
                lbEventParameters.Clear();
                tbEventParameterValue.Text = string.Empty;
                return;
            }

            var triggerCondition = (TriggerCondition)lbEvents.SelectedItem.Tag;
            TriggerEventType triggerEventType = null;
            if (triggerCondition.ConditionIndex < map.EditorConfig.TriggerEventTypes.Count)
                triggerEventType = map.EditorConfig.TriggerEventTypes[triggerCondition.ConditionIndex];

            selEventType.Text = triggerCondition.ConditionIndex + " " + (triggerEventType == null ? "Unknown" : triggerEventType.Name);
            panelEventDescription.Text = triggerEventType == null ? "Unknown event. It has most likely been added with another editor." : triggerEventType.Description;

            lbEventParameters.Clear();
            if (triggerEventType == null)
            {
                lbEventParameters.AddItem(new XNAListBoxItem() { Text = "Parameter 0", Tag = EVENT_PARAM_FIRST });
                lbEventParameters.AddItem(new XNAListBoxItem() { Text = "Parameter 1", Tag = EVENT_PARAM_SECOND });
            }
            else
            {
                if (triggerEventType.P1Type != TriggerParamType.Unused)
                    lbEventParameters.AddItem(new XNAListBoxItem() { Text = triggerEventType.P1Type.ToString(), Tag = EVENT_PARAM_FIRST });

                if (triggerEventType.P2Type != TriggerParamType.Unused)
                    lbEventParameters.AddItem(new XNAListBoxItem() { Text = triggerEventType.P2Type.ToString(), Tag = EVENT_PARAM_SECOND });
            }

            LbEventParameters_SelectedIndexChanged(this, EventArgs.Empty);

            lbEventParameters.SelectedIndexChanged += LbEventParameters_SelectedIndexChanged;
        }

        private void LbEventParameters_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbEventParameters.SelectedItem == null || editedTrigger == null || lbEvents.SelectedItem == null)
            {
                tbEventParameterValue.Text = string.Empty;
                return;
            }

            int paramNumber = (int)lbActionParameters.SelectedItem.Tag;
            if (paramNumber == EVENT_PARAM_FIRST)
            {
                tbEventParameterValue.Text = editedTrigger.Conditions[lbEvents.SelectedIndex].Parameter1.ToString();
            }
            else
            {
                tbEventParameterValue.Text = editedTrigger.Conditions[lbEvents.SelectedIndex].Parameter2.ToString();
            }

            // TODO figure out the parameter contexts and display more relevant information
        }

        private void AddEvent(TriggerCondition condition)
        {
            if (condition.ConditionIndex >= map.EditorConfig.TriggerEventTypes.Count)
            {
                lbEvents.AddItem(new XNAListBoxItem() { Text = condition.ConditionIndex + " Unknown", Tag = condition });
                return;
            }

            lbEvents.AddItem(new XNAListBoxItem() { Text = condition.ConditionIndex + " " + map.EditorConfig.TriggerEventTypes[condition.ConditionIndex].Name, Tag = condition });
        }

        private void AddAction(TriggerAction action)
        {
            if (action.ActionIndex >= map.EditorConfig.TriggerActionTypes.Count)
            {
                lbActions.AddItem(new XNAListBoxItem() { Text = action.ActionIndex + " Unknown", Tag = action });
                return;
            }

            lbActions.AddItem(new XNAListBoxItem() { Text = action.ActionIndex + " " + map.EditorConfig.TriggerActionTypes[action.ActionIndex].Name, Tag = action });
        }
    }
}
