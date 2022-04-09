using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.UI.Windows
{
    public class TriggersWindow : INItializableWindow
    {
        private const int EVENT_PARAM_FIRST = 0;
        private const int EVENT_PARAM_SECOND = 1;

        public TriggersWindow(WindowManager windowManager, Map map, EditorState editorState, ICursorActionTarget cursorActionTarget) : base(windowManager)
        {
            this.map = map;
            this.editorState = editorState;

            placeCellTagCursorAction = new PlaceCellTagCursorAction(cursorActionTarget);
            changeAttachedTagCursorAction = new ChangeAttachedTagCursorAction(cursorActionTarget);
        }

        private readonly Map map;
        private readonly PlaceCellTagCursorAction placeCellTagCursorAction;
        private readonly ChangeAttachedTagCursorAction changeAttachedTagCursorAction;
        private readonly EditorState editorState;

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

        private SelectEventWindow selectEventWindow;
        private SelectActionWindow selectActionWindow;
        private SelectTeamTypeWindow selectTeamTypeWindow;
        private SelectTriggerWindow selectTriggerWindow;
        private SelectGlobalVariableWindow selectGlobalVariableWindow;
        private SelectLocalVariableWindow selectLocalVariableWindow;
        private SelectHouseWindow selectHouseWindow;

        private XNAContextMenu contextMenu;

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
            FindChild<EditorButton>("btnAttachToObjects").LeftClick += BtnAttachToObjects_LeftClick;
            FindChild<EditorButton>("btnViewAttachedObjects").LeftClick += BtnViewAttachedObjects_LeftClick;
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

            FindChild<EditorButton>("btnNewTrigger").LeftClick += BtnNewTrigger_LeftClick;
            FindChild<EditorButton>("btnDeleteTrigger").LeftClick += BtnDeleteTrigger_LeftClick;
            FindChild<EditorButton>("btnCloneTrigger").LeftClick += BtnCloneTrigger_LeftClick;
            FindChild<EditorButton>("btnPlaceCellTag").LeftClick += BtnPlaceCellTag_LeftClick;

            FindChild<EditorButton>("btnAddEvent").LeftClick += BtnAddEvent_LeftClick;
            FindChild<EditorButton>("btnDeleteEvent").LeftClick += BtnDeleteEvent_LeftClick;

            FindChild<EditorButton>("btnAddAction").LeftClick += BtnAddAction_LeftClick;
            FindChild<EditorButton>("btnDeleteAction").LeftClick += BtnDeleteAction_LeftClick;

            FindChild<EditorButton>("btnActionParameterValuePreset").LeftClick += BtnActionParameterValuePreset_LeftClick;
            FindChild<EditorButton>("btnEventParameterValuePreset").LeftClick += BtnEventParameterValuePreset_LeftClick;

            selectEventWindow = new SelectEventWindow(WindowManager, map);
            var eventWindowDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectEventWindow);
            eventWindowDarkeningPanel.Hidden += EventWindowDarkeningPanel_Hidden;

            selectActionWindow = new SelectActionWindow(WindowManager, map);
            var actionWindowDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectActionWindow);
            actionWindowDarkeningPanel.Hidden += ActionWindowDarkeningPanel_Hidden;

            selectTeamTypeWindow = new SelectTeamTypeWindow(WindowManager, map);
            var teamTypeWindowDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectTeamTypeWindow);
            teamTypeWindowDarkeningPanel.Hidden += TeamTypeWindowDarkeningPanel_Hidden;

            selectTriggerWindow = new SelectTriggerWindow(WindowManager, map);
            var triggerWindowDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectTriggerWindow);
            triggerWindowDarkeningPanel.Hidden += TriggerWindowDarkeningPanel_Hidden;

            selectGlobalVariableWindow = new SelectGlobalVariableWindow(WindowManager, map);
            var globalVariableDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectGlobalVariableWindow);
            globalVariableDarkeningPanel.Hidden += GlobalVariableDarkeningPanel_Hidden;

            selectLocalVariableWindow = new SelectLocalVariableWindow(WindowManager, map);
            var localVariableDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectLocalVariableWindow);
            localVariableDarkeningPanel.Hidden += LocalVariableDarkeningPanel_Hidden;

            selectHouseWindow = new SelectHouseWindow(WindowManager, map);
            var houseDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectHouseWindow);
            houseDarkeningPanel.Hidden += HouseDarkeningPanel_Hidden;

            contextMenu = new XNAContextMenu(WindowManager);
            contextMenu.Name = nameof(contextMenu);
            contextMenu.Width = tbActionParameterValue.Width;
            AddChild(contextMenu);

            lbTriggers.SelectedIndexChanged += LbTriggers_SelectedIndexChanged;
        }

        private void BtnAttachToObjects_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);
            if (tag == null)
            {
                EditorMessageBox.Show(WindowManager, "No tag found",
                    $"The selected trigger '{editedTrigger.Name}' has no" +
                    $"associated tag. As such, it cannot be attached to any objects." + Environment.NewLine + Environment.NewLine +
                    "This should never happen, have you modified the map with another editor?",
                    MessageBoxButtons.OK);

                return;
            }

            changeAttachedTagCursorAction.TagToAttach = tag;
            editorState.CursorAction = changeAttachedTagCursorAction;
        }

        #region Viewing linked objects

        private void BtnViewAttachedObjects_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);
            if (tag == null)
            {
                EditorMessageBox.Show(WindowManager, "No tag found", 
                    $"The selected trigger '{editedTrigger.Name}' has no" +
                    $"associated tag. As such, it is not attached to any objects.",
                    MessageBoxButtons.OK);

                return;
            }

            var objectList = new List<TechnoBase>();
            map.Infantry.ForEach(inf => AddObjectToListIfLinkedToTag(inf, objectList, tag));
            map.Units.ForEach(unit => AddObjectToListIfLinkedToTag(unit, objectList, tag));
            map.Structures.ForEach(structure => AddObjectToListIfLinkedToTag(structure, objectList, tag));
            map.Aircraft.ForEach(aircraft => AddObjectToListIfLinkedToTag(aircraft, objectList, tag));

            var stringBuilder = new StringBuilder(Environment.NewLine + Environment.NewLine);

            if (objectList.Count == 0)
            {
                stringBuilder.Append("No attached objects found." + Environment.NewLine);
            }
            else
            {
                objectList.ForEach(techno =>
                {
                    switch (techno.WhatAmI())
                    {
                        case RTTIType.Aircraft:
                            AppendToStringBuilder((Aircraft)techno, stringBuilder);
                            break;
                        case RTTIType.Building:
                            AppendToStringBuilder((Structure)techno, stringBuilder);
                            break;
                        case RTTIType.Infantry:
                            AppendToStringBuilder((Infantry)techno, stringBuilder);
                            break;
                        case RTTIType.Unit:
                            AppendToStringBuilder((Unit)techno, stringBuilder);
                            break;
                        default:
                            throw new NotImplementedException("Unknown RTTI type encountered when listing linked objects for a trigger.");
                    }
                });
            }

            var celltag = map.CellTags.Find(ct => ct.Tag == tag);
            if (celltag != null)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append("The trigger is linked to one or more celltags (first match at " + celltag.Position + ").");
            }

            EditorMessageBox.Show(WindowManager, "Linked Objects",
                $"The selected trigger '{editedTrigger.Name}' is linked to the following objects: " + stringBuilder.ToString(),
                MessageBoxButtons.OK);

            return;
        }

        private void AppendToStringBuilder<T>(Techno<T> techno, StringBuilder stringBuilder) where T : GameObjectType
        {
            string rtti = techno.WhatAmI().ToString();
            string name = techno.ObjectType.Name;
            string position = techno.Position.ToString();

            stringBuilder.Append($"{rtti}: {name} at {position}{Environment.NewLine}");
        }

        private void AddObjectToListIfLinkedToTag(TechnoBase techno, List<TechnoBase> technoList, Tag tag)
        {
            if (techno.AttachedTag == tag)
                technoList.Add(techno);
        }

        #endregion

        private void BtnEventParameterValuePreset_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbEvents.SelectedItem == null || lbEventParameters.SelectedItem == null)
                return;

            var triggerEvent = (TriggerCondition)lbEvents.SelectedItem.Tag;
            var triggerEventType = GetTriggerEventType(triggerEvent.ConditionIndex);
            int paramIndex = (int)lbEventParameters.SelectedItem.Tag;

            if (triggerEventType == null)
                return;

            TriggerParamType triggerParamType = paramIndex == EVENT_PARAM_FIRST ? triggerEventType.P1Type : triggerEventType.P2Type;
            int paramValue = paramIndex == EVENT_PARAM_FIRST ? triggerEvent.Parameter1 : triggerEvent.Parameter2;

            switch (triggerParamType)
            {
                case TriggerParamType.GlobalVariable:
                    GlobalVariable existingGlobalVariable = map.Rules.GlobalVariables.Find(gv => gv.Index == paramValue);
                    selectGlobalVariableWindow.IsForEvent = true;
                    selectGlobalVariableWindow.Open(existingGlobalVariable);
                    break;
                case TriggerParamType.LocalVariable:
                    LocalVariable existingLocalVariable = map.LocalVariables.Find(lv => lv.Index == paramValue);
                    selectLocalVariableWindow.IsForEvent = true;
                    selectLocalVariableWindow.Open(existingLocalVariable);
                    break;
                case TriggerParamType.House:
                    selectHouseWindow.IsForEvent = true;
                    if (paramValue > -1 || paramValue < map.Houses.Count)
                        selectHouseWindow.Open(map.Houses[paramValue]);
                    else
                        selectHouseWindow.Open(null);
                    break;
                default:
                    break;
            }
        }

        private void BtnActionParameterValuePreset_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbActions.SelectedItem == null || lbActionParameters.SelectedItem == null)
                return;

            var triggerAction = (TriggerAction)lbActions.SelectedItem.Tag;
            var triggerActionType = GetTriggerActionType(triggerAction.ActionIndex);
            int paramIndex = (int)lbActionParameters.SelectedItem.Tag;

            if (triggerActionType == null)
                return;

            switch (triggerActionType.Parameters[paramIndex].TriggerParamType)
            {
                case TriggerParamType.TeamType:
                    TeamType existingTeamType = map.TeamTypes.Find(tt => tt.ININame == triggerAction.Parameters[paramIndex]);
                    selectTeamTypeWindow.IsForEvent = false;
                    selectTeamTypeWindow.Open(existingTeamType);
                    break;
                case TriggerParamType.Trigger:
                    Trigger existingTrigger = map.Triggers.Find(tt => tt.ID == triggerAction.Parameters[paramIndex]);
                    selectTriggerWindow.Open(existingTrigger);
                    break;
                case TriggerParamType.GlobalVariable:
                    GlobalVariable existingGlobalVariable = map.Rules.GlobalVariables.Find(gv => gv.Index == Conversions.IntFromString(triggerAction.Parameters[paramIndex], -1));
                    selectGlobalVariableWindow.IsForEvent = false;
                    selectGlobalVariableWindow.Open(existingGlobalVariable);
                    break;
                case TriggerParamType.LocalVariable:
                    LocalVariable existingLocalVariable = map.LocalVariables.Find(lv => lv.Index == Conversions.IntFromString(triggerAction.Parameters[paramIndex], -1));
                    selectLocalVariableWindow.IsForEvent = false;
                    selectLocalVariableWindow.Open(existingLocalVariable);
                    break;
                case TriggerParamType.House:
                    int houseIndex = Conversions.IntFromString(triggerAction.Parameters[paramIndex], -1);
                    selectHouseWindow.IsForEvent = false;
                    if (houseIndex > -1 && houseIndex < map.Houses.Count)
                        selectHouseWindow.Open(map.Houses[houseIndex]);
                    else
                        selectHouseWindow.Open(null);
                    break;
                default:
                    break;
            }
        }

        private void HouseDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (selectHouseWindow.SelectedObject == null)
                return;

            int houseIndex = map.Houses.FindIndex(h => h == selectHouseWindow.SelectedObject);
            AssignParamValue(selectHouseWindow.IsForEvent, houseIndex);
        }

        private void LocalVariableDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (selectLocalVariableWindow.SelectedObject == null)
                return;

            int localVariableIndex = selectLocalVariableWindow.SelectedObject.Index;
            AssignParamValue(selectLocalVariableWindow.IsForEvent, localVariableIndex);
        }

        private void GlobalVariableDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (selectGlobalVariableWindow.SelectedObject == null)
                return;

            int globalVariableIndex = selectGlobalVariableWindow.SelectedObject.Index;
            AssignParamValue(selectGlobalVariableWindow.IsForEvent, globalVariableIndex);
        }

        private void AssignParamValue(bool isForEvent, int paramValue)
        {
            if (isForEvent)
            {
                GetTriggerEventAndParamIndex(out TriggerCondition triggerCondition, out int paramIndex);
                if (paramIndex == EVENT_PARAM_FIRST)
                    triggerCondition.Parameter1 = paramValue;
                else
                    triggerCondition.Parameter2 = paramValue;
            }
            else
            {
                GetTriggerActionAndParamIndex(out TriggerAction triggerAction, out int paramIndex);
                triggerAction.Parameters[paramIndex] = paramValue.ToString();
            }

            EditTrigger(editedTrigger);
        }

        private void TriggerWindowDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (selectTriggerWindow.SelectedObject == null)
                return;

            GetTriggerActionAndParamIndex(out TriggerAction triggerAction, out int paramIndex);
            triggerAction.Parameters[paramIndex] = selectTriggerWindow.SelectedObject.ID;
            EditTrigger(editedTrigger);
        }

        private void TeamTypeWindowDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (selectTeamTypeWindow.SelectedObject == null)
                return;

            GetTriggerActionAndParamIndex(out TriggerAction triggerAction, out int paramIndex);
            triggerAction.Parameters[paramIndex] = selectTeamTypeWindow.SelectedObject.ININame;
            EditTrigger(editedTrigger);
        }

        private void GetTriggerActionAndParamIndex(out TriggerAction triggerAction, out int paramIndex)
        {
            triggerAction = (TriggerAction)lbActions.SelectedItem.Tag;
            paramIndex = (int)lbActionParameters.SelectedItem.Tag;
        }

        private void GetTriggerEventAndParamIndex(out TriggerCondition triggerEvent, out int paramIndex)
        {
            triggerEvent = (TriggerCondition)lbEvents.SelectedItem.Tag;
            paramIndex = (int)lbEventParameters.SelectedItem.Tag;
        }

        private void BtnNewTrigger_LeftClick(object sender, EventArgs e)
        {
            var newTrigger = new Trigger(map.GetNewUniqueInternalId()) { Name = "New trigger" };
            map.Triggers.Add(newTrigger);
            map.Tags.Add(new Tag() { ID = map.GetNewUniqueInternalId(), Name = "New tag", Trigger = newTrigger });
            ListTriggers();
            SelectLastTrigger();
        }

        private void BtnCloneTrigger_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            var clone = editedTrigger.Clone(map.GetNewUniqueInternalId());
            map.Triggers.Add(clone);
            map.Tags.Add(new Tag() { ID = map.GetNewUniqueInternalId(), Name = clone.Name + " (tag)", Trigger = clone });
            ListTriggers();
            SelectLastTrigger();
        }

        private void BtnDeleteTrigger_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            map.Triggers.Remove(editedTrigger);
            map.Triggers.ForEach(t => { if (t.LinkedTrigger == editedTrigger) t.LinkedTrigger = null; });
            map.Tags.RemoveAll(t => t.Trigger == editedTrigger);
            editedTrigger = null;

            ListTriggers();
        }

        private void BtnPlaceCellTag_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null)
                return;

            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);
            if (tag == null)
            {
                return;
            }

            placeCellTagCursorAction.Tag = tag;
            editorState.CursorAction = placeCellTagCursorAction;
        }

        private void SelectLastTrigger()
        {
            lbTriggers.SelectedIndex = map.Triggers.Count - 1;
            lbTriggers.ScrollToBottom();
        }

        private void EventWindowDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbEvents.SelectedItem == null || selectEventWindow.SelectedObject == null)
                return;

            TriggerCondition condition = editedTrigger.Conditions[lbEvents.SelectedIndex];
            condition.ConditionIndex = selectEventWindow.SelectedObject.ID;

            if (selectEventWindow.SelectedObject.P1Type == TriggerParamType.Unused)
                condition.Parameter1 = 0;

            if (selectEventWindow.SelectedObject.P2Type == TriggerParamType.Unused)
                condition.Parameter2 = 0;

            EditTrigger(editedTrigger);
        }

        private void ActionWindowDarkeningPanel_Hidden(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbActions.SelectedItem == null || selectActionWindow.SelectedObject == null)
                return;

            TriggerActionType triggerActionType = selectActionWindow.SelectedObject;
            TriggerAction action = editedTrigger.Actions[lbActions.SelectedIndex];
            action.ActionIndex = selectActionWindow.SelectedObject.ID;

            for (int i = 0; i < TriggerActionType.MAX_PARAM_COUNT; i++)
            {
                if ((int)triggerActionType.Parameters[i].TriggerParamType < 0)
                {
                    action.Parameters[i] = Math.Abs((int)triggerActionType.Parameters[i].TriggerParamType).ToString(CultureInfo.InvariantCulture);
                    continue;
                }

                if (triggerActionType.Parameters[i].TriggerParamType == TriggerParamType.Unknown)
                {
                    action.Parameters[i] = "0";
                    continue;
                }
            }

            EditTrigger(editedTrigger);
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

            LbTriggers_SelectedIndexChanged(this, EventArgs.Empty);
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
            if (tag == null)
            {
                ddType.AllowDropDown = false;

                if (ddType.Items.Count < 4)
                    ddType.AddItem("Error: No tag exists for this trigger!");
            }
            else
            {
                ddType.AllowDropDown = true;

                if (ddType.Items.Count > 3)
                    ddType.Items.RemoveAt(3);
            }

            tbName.Text = editedTrigger.Name;
            ddHouse.SelectedIndex = map.GetHouses().FindIndex(h => h.ININame == trigger.House);
            ddType.SelectedIndex = tag == null ? 3 : tag.Repeating;
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
            var tag = map.Tags.Find(t => t.Trigger == editedTrigger);
            if (tag != null)
                tag.Name = tbName.Text + " (tag)";

            editedTrigger.Name = tbName.Text;
            lbTriggers.SelectedItem.Text = tbName.Text;
        }

        private void LbActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            lbActionParameters.SelectedIndexChanged -= LbActionParameters_SelectedIndexChanged;
            selActionType.LeftClick -= SelActionType_LeftClick;

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

            if (lbActionParameters.SelectedItem == null && lbActionParameters.Items.Count > 0)
                lbActionParameters.SelectedIndex = 0;

            LbActionParameters_SelectedIndexChanged(this, EventArgs.Empty);

            lbActionParameters.SelectedIndexChanged += LbActionParameters_SelectedIndexChanged;
            selActionType.LeftClick += SelActionType_LeftClick;
        }

        private void SelActionType_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbActions.SelectedItem == null)
                return;

            int actionTypeIndex = editedTrigger.Actions[lbActions.SelectedIndex].ActionIndex;
            selectActionWindow.Open(GetTriggerActionType(actionTypeIndex));
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
            tbActionParameterValue.TextColor = GetParamValueColor(triggerAction.Parameters[paramNumber], triggerParamType);

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

            var triggerActionType = GetTriggerActionType(triggerAction.ActionIndex);

            if (triggerActionType != null && 
                triggerActionType.Parameters[paramNumber].TriggerParamType == TriggerParamType.WaypointZZ)
            {
                // Write waypoint with A-ZZ notation
                value = Helpers.WaypointNumberToAlphabeticalString(Conversions.IntFromString(value, 0));
            }

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
            selEventType.LeftClick -= SelEventType_LeftClick;

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

            if (lbEventParameters.SelectedItem == null && lbEventParameters.Items.Count > 0)
                lbEventParameters.SelectedIndex = 0;

            LbEventParameters_SelectedIndexChanged(this, EventArgs.Empty);

            lbEventParameters.SelectedIndexChanged += LbEventParameters_SelectedIndexChanged;
            selEventType.LeftClick += SelEventType_LeftClick;
        }

        private void SelEventType_LeftClick(object sender, EventArgs e)
        {
            if (editedTrigger == null || lbEvents.SelectedItem == null)
                return;

            int eventTypeIndex = editedTrigger.Conditions[lbEvents.SelectedIndex].ConditionIndex;
            selectEventWindow.Open(GetTriggerEventType(eventTypeIndex));
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

        private Color GetParamValueColor(string paramValue, TriggerParamType paramType)
        {
            bool intParseSuccess = int.TryParse(paramValue, NumberStyles.None, CultureInfo.InvariantCulture, out int intValue);

            switch (paramType)
            {
                case TriggerParamType.House:
                    if (intParseSuccess)
                    {
                        var houses = map.GetHouses();
                        if (intValue >= houses.Count)
                            goto case TriggerParamType.Unused;

                        return houses[intValue].XNAColor;
                    }
                    goto case TriggerParamType.Unused;

                case TriggerParamType.Unused:
                default:
                    return UISettings.ActiveSettings.AltColor;
            }
        }

        private string GetParamValueText(string paramValue, TriggerParamType paramType)
        {
            bool intParseSuccess = int.TryParse(paramValue, NumberStyles.None, CultureInfo.InvariantCulture, out int intValue);
            if (paramValue == null)
                paramValue = string.Empty;

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

                    if (intValue >= map.Rules.GlobalVariables.Count)
                        return intValue + " - nonexistant variable";

                    return intValue + " " + map.Rules.GlobalVariables[intValue].Name;
                case TriggerParamType.LocalVariable:
                    if (!intParseSuccess)
                        return paramValue;

                    if (intValue >= map.LocalVariables.Count)
                        return intValue + " - nonexistant variable";

                    return intValue + " " + map.LocalVariables[intValue].Name;
                case TriggerParamType.WaypointZZ:
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
