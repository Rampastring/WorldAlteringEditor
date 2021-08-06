using System;
using System.Collections.Generic;
using System.Globalization;
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
        private EditorTextBox tbActionParameterValue;

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
            tbActionParameterValue = FindChild<EditorTextBox>(nameof(tbActionParameterValue));

            ddType.AddItem("0 - one-time, single-object condition");
            ddType.AddItem("1 - one-time, multi-object condition");
            ddType.AddItem("2 - repeating, single-object condition");

            lbEvents.AllowMultiLineItems = false;
            lbActions.AllowMultiLineItems = false;

            FindChild<EditorButton>("btnAddEvent").LeftClick += BtnAddEvent_LeftClick;
            FindChild<EditorButton>("btnDeleteEvent").LeftClick += BtnDeleteEvent_LeftClick;

            FindChild<EditorButton>("btnAddAction").LeftClick += BtnAddAction_LeftClick;
            FindChild<EditorButton>("btnDeleteAction").LeftClick += BtnDeleteAction_LeftClick;

            lbTriggers.SelectedIndexChanged += LbTriggers_SelectedIndexChanged;
        }

        private void BtnAddAction_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            editedTrigger.Actions.Add(new TriggerAction());
            EditTrigger(editedTrigger);
            lbActions.SelectedIndex = lbActions.Items.Count - 1;
        }

        private void BtnDeleteAction_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbActions.SelectedItem == null)
                return;

            editedTrigger.Actions.RemoveAt(lbActions.SelectedIndex);
            EditTrigger(editedTrigger);
        }

        private void BtnAddEvent_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            editedTrigger.Conditions.Add(new TriggerCondition());
            EditTrigger(editedTrigger);
            lbEvents.SelectedIndex = lbEvents.Items.Count - 1;
        }

        private void BtnDeleteEvent_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbEvents.SelectedItem == null)
                return;

            editedTrigger.Conditions.RemoveAt(lbEvents.SelectedIndex);
            EditTrigger(editedTrigger);
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
            tbName.TextChanged -= TbName_TextChanged;
            ddHouse.SelectedIndexChanged -= DdHouse_SelectedIndexChanged;
            ddType.SelectedIndexChanged -= DdType_SelectedIndexChanged;
            chkDisabled.CheckedChanged -= ChkDisabled_CheckedChanged;

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
            tbName.TextChanged += TbName_TextChanged;
            ddHouse.SelectedIndexChanged += DdHouse_SelectedIndexChanged;
            ddType.SelectedIndexChanged += DdType_SelectedIndexChanged;
            chkDisabled.CheckedChanged += ChkDisabled_CheckedChanged;
        }

        private void DdType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);
            if (tag != null)
                tag.Repeating = ddType.SelectedIndex;
        }

        private void DdHouse_SelectedIndexChanged(object sender, EventArgs e)
        {
            editedTrigger.House = ddHouse.SelectedItem.Text;
        }

        private void ChkDisabled_CheckedChanged(object sender, EventArgs e)
        {
            editedTrigger.Disabled = chkDisabled.Checked;
        }

        private void TbName_TextChanged(object sender, EventArgs e)
        {
            editedTrigger.Name = tbName.Text;
            lbTriggers.SelectedItem.Text = tbName.Text;
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
            tbActionParameterValue.TextChanged -= TbActionParameterValue_TextChanged;

            if (lbActionParameters.SelectedItem == null || editedTrigger == null || lbActions.SelectedItem == null)
            {
                tbActionParameterValue.Text = string.Empty;
                return;
            }

            TriggerAction triggerAction = editedTrigger.Actions[lbActions.SelectedIndex];
            int paramNumber = (int)lbActionParameters.SelectedItem.Tag;
            var triggerActionType = GetTriggerActionType(triggerAction.ActionIndex);
            var triggerParamType = TriggerParamType.Unknown;
            if (triggerActionType != null)
            {
                triggerParamType = triggerActionType.Parameters[paramNumber].TriggerParamType;
            }

            tbActionParameterValue.Text = GetParamValueText(triggerAction.Parameters[paramNumber], triggerParamType);

            tbActionParameterValue.TextChanged += TbActionParameterValue_TextChanged;
        }

        private void TbActionParameterValue_TextChanged(object sender, EventArgs e)
        {
            if (lbActionParameters.SelectedItem == null || editedTrigger == null || lbActions.SelectedItem == null)
            {
                return;
            }

            int paramNumber = (int)lbActionParameters.SelectedItem.Tag;
            var triggerAction = (TriggerAction)lbActions.SelectedItem.Tag;

            int spaceIndex = tbActionParameterValue.Text.IndexOf(' ');
            string value = tbActionParameterValue.Text;
            if (spaceIndex > -1)
                value = value.Substring(0, spaceIndex);

            triggerAction.Parameters[paramNumber] = value;
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
            tbEventParameterValue.TextChanged -= TbEventParameterValue_TextChanged;

            if (lbEventParameters.SelectedItem == null || editedTrigger == null || lbEvents.SelectedItem == null)
            {
                tbEventParameterValue.Text = string.Empty;
                return;
            }

            var triggerEventType = GetTriggerEventType(editedTrigger.Conditions[lbEvents.SelectedIndex].ConditionIndex);
            var triggerParamType = TriggerParamType.Unknown;
            int paramValue = -1;
            int paramNumber = (int)lbEventParameters.SelectedItem.Tag;
            if (paramNumber == EVENT_PARAM_FIRST)
            {
                paramValue = editedTrigger.Conditions[lbEvents.SelectedIndex].Parameter1;

                if (triggerEventType != null)
                    triggerParamType = triggerEventType.P1Type;
            }
            else
            {
                paramValue = editedTrigger.Conditions[lbEvents.SelectedIndex].Parameter2;

                if (triggerEventType != null)
                    triggerParamType = triggerEventType.P2Type;
            }

            tbEventParameterValue.Text = GetParamValueText(paramValue.ToString(), triggerParamType);

            tbEventParameterValue.TextChanged += TbEventParameterValue_TextChanged;
        }

        private void TbEventParameterValue_TextChanged(object sender, EventArgs e)
        {
            if (lbEventParameters.SelectedItem == null || editedTrigger == null || lbEvents.SelectedItem == null)
            {
                return;
            }

            int paramNumber = (int)lbEventParameters.SelectedItem.Tag;

            var triggerCondition = (TriggerCondition)lbEvents.SelectedItem.Tag;
            if (paramNumber == EVENT_PARAM_FIRST)
                triggerCondition.Parameter1 = tbEventParameterValue.Value;
            else
                triggerCondition.Parameter2 = tbEventParameterValue.Value;
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

        private TriggerEventType GetTriggerEventType(int index)
        {
            if (index >= map.EditorConfig.TriggerEventTypes.Count)
                return null;

            return map.EditorConfig.TriggerEventTypes[index];
        }

        private TriggerActionType GetTriggerActionType(int index)
        {
            if (index >= map.EditorConfig.TriggerActionTypes.Count)
                return null;

            return map.EditorConfig.TriggerActionTypes[index];
        }

        private string GetParamValueText(string paramValue, TriggerParamType paramType)
        {
            bool intParseSuccess = int.TryParse(paramValue, NumberStyles.None, CultureInfo.InvariantCulture, out int intValue);

            switch (paramType)
            {
                case TriggerParamType.House:
                    if (intParseSuccess)
                    {
                        var houses = map.GetHouses();
                        if (intValue >= houses.Count)
                            return intValue.ToString() + " - Unknown House";

                        return intValue + " " + houses[intValue].ININame;
                    }

                    return paramValue;
                case TriggerParamType.GlobalVariable:
                    if (!intParseSuccess)
                        return paramValue;

                    if (intValue >= map.Rules.GlobalVariableNames.Count)
                        return intValue + " - invalid variable";

                    return intValue + " " + map.Rules.GlobalVariableNames[intValue];
                case TriggerParamType.Waypoint:
                    if (!intParseSuccess)
                        return Helpers.GetWaypointNumberFromAlphabeticalString(paramValue).ToString();

                    return intValue.ToString();
                case TriggerParamType.TeamType:
                    TeamType teamType = map.TeamTypes.Find(t => t.ININame == paramValue);
                    if (teamType == null)
                        return paramValue;

                    return paramValue + " " + teamType.Name;
                case TriggerParamType.Trigger:
                    Trigger trigger = map.Triggers.Find(t => t.ID == paramValue);
                    if (trigger == null)
                        return paramValue;

                    return paramValue + " " + trigger.Name;
                case TriggerParamType.Building:
                    return GetObjectValueText(RTTIType.Building, map.Rules.BuildingTypes, paramValue);
                case TriggerParamType.Aircraft:
                    return GetObjectValueText(RTTIType.Aircraft, map.Rules.AircraftTypes, paramValue);
                case TriggerParamType.Infantry:
                    return GetObjectValueText(RTTIType.Infantry, map.Rules.InfantryTypes, paramValue);
                case TriggerParamType.Unit:
                    return GetObjectValueText(RTTIType.Unit, map.Rules.UnitTypes, paramValue);
                case TriggerParamType.Boolean:
                default:
                    return paramValue;
            }
        }

        private string GetObjectValueText<T>(RTTIType rtti, List<T> objectTypeList, string paramValue) where T : TechnoType
        {
            bool intParseSuccess = int.TryParse(paramValue, NumberStyles.None, CultureInfo.InvariantCulture, out int intValue);

            if (!intParseSuccess)
                return paramValue;

            if (intValue >= objectTypeList.Count)
            {
                switch (rtti)
                {
                    case RTTIType.Aircraft:
                        return intValue + " - Unknown Aircraft";
                    case RTTIType.Building:
                        return intValue + " - Unknown Building";
                    case RTTIType.Infantry:
                        return intValue + " - Unknown Infantry";
                    case RTTIType.Unit:
                        return intValue + " - Unknown Unit";
                    default:
                        return intValue + " - Unknown Object";
                }
            }

            return intValue + " " + objectTypeList[intValue].GetEditorDisplayName();
        }
    }
}
