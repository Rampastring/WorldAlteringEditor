using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Reflection;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class TeamTypesWindow : INItializableWindow
    {
        public TeamTypesWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorListBox lbTeamTypes;
        private EditorTextBox tbName;
        private XNADropDown ddVeteranLevel;
        private XNADropDown ddHouse;
        private EditorNumberTextBox tbPriority;
        private EditorNumberTextBox tbMax;
        private EditorNumberTextBox tbTechLevel;
        private EditorNumberTextBox tbGroup;
        private EditorNumberTextBox tbWaypoint;
        private EditorPopUpSelector selTaskForce;
        private EditorPopUpSelector selScript;
        private EditorPopUpSelector selTag;

        private TeamType editedTeamType;
        private List<XNACheckBox> checkBoxes = new List<XNACheckBox>();

        public override void Initialize()
        {
            Name = nameof(TeamTypesWindow);
            base.Initialize();

            lbTeamTypes = FindChild<EditorListBox>(nameof(lbTeamTypes));
            tbName = FindChild<EditorTextBox>(nameof(tbName));
            ddVeteranLevel = FindChild<XNADropDown>(nameof(ddVeteranLevel));
            ddHouse = FindChild<XNADropDown>(nameof(ddHouse));
            tbPriority = FindChild<EditorNumberTextBox>(nameof(tbPriority));
            tbMax = FindChild<EditorNumberTextBox>(nameof(tbMax));
            tbTechLevel = FindChild<EditorNumberTextBox>(nameof(tbTechLevel));
            tbGroup = FindChild<EditorNumberTextBox>(nameof(tbGroup));
            tbWaypoint = FindChild<EditorNumberTextBox>(nameof(tbWaypoint));
            selTaskForce = FindChild<EditorPopUpSelector>(nameof(selTaskForce));
            selScript = FindChild<EditorPopUpSelector>(nameof(selScript));
            selTag = FindChild<EditorPopUpSelector>(nameof(selTag));

            var panelBooleans = FindChild<EditorPanel>("panelBooleans");
            AddBooleanProperties(panelBooleans);

            ddVeteranLevel.AddItem("Regular");
            ddVeteranLevel.AddItem("Veteran");
            ddVeteranLevel.AddItem("Elite");

            lbTeamTypes.SelectedIndexChanged += LbTeamTypes_SelectedIndexChanged;
        }

        private void LbTeamTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTeamTypes.SelectedItem == null)
            {
                editedTeamType = null;
                EditTeamType(null);
                return;
            }

            EditTeamType((TeamType)lbTeamTypes.SelectedItem.Tag);
        }

        private void AddBooleanProperties(EditorPanel panelBooleans)
        {
            var type = typeof(TeamType);
            PropertyInfo[] properties = type.GetProperties();

            int currentColumnRight = 0;
            int currentColumnX = Constants.UIEmptySideSpace;
            XNACheckBox previousCheckBoxOnColumn = null;

            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(bool))
                    continue;

                if (property.GetSetMethod() == null || property.GetGetMethod() == null)
                    continue;

                var checkBox = new XNACheckBox(WindowManager);
                checkBox.Tag = property;
                checkBox.CheckedChanged += (s, e) =>
                {
                    if (editedTeamType != null)
                        property.SetValue(editedTeamType, checkBox.Checked);
                };
                checkBox.Text = property.Name;
                panelBooleans.AddChild(checkBox);
                checkBoxes.Add(checkBox);

                if (previousCheckBoxOnColumn == null)
                {
                    checkBox.Y = Constants.UIEmptyTopSpace;
                    checkBox.X = currentColumnX;
                }
                else
                {
                    checkBox.Y = previousCheckBoxOnColumn.Bottom + Constants.UIVerticalSpacing;
                    checkBox.X = currentColumnX;

                    // Start new column
                    if (checkBox.Bottom > panelBooleans.Height - Constants.UIEmptyBottomSpace)
                    {
                        currentColumnX = currentColumnRight + Constants.UIHorizontalSpacing * 2;
                        checkBox.Y = Constants.UIEmptyTopSpace;
                        checkBox.X = currentColumnX;
                        currentColumnRight = 0;
                    }
                }

                previousCheckBoxOnColumn = checkBox;
                currentColumnRight = Math.Max(currentColumnRight, checkBox.Right);
            }
        }

        public void Open()
        {
            Show();
            ListHouses();
            ListTeamTypes();
        }

        private void ListHouses()
        {
            ddHouse.Items.Clear();
            map.GetHouses().ForEach(h => ddHouse.AddItem(h.ININame, h.XNAColor));
        }

        private void ListTeamTypes()
        {
            lbTeamTypes.Clear();

            foreach (var teamType in map.TeamTypes)
            {
                lbTeamTypes.AddItem(new XNAListBoxItem() { Text = teamType.Name, Tag = teamType });
            }
        }

        private void EditTeamType(TeamType teamType)
        {
            editedTeamType = teamType;

            if (editedTeamType == null)
            {
                tbName.Text = string.Empty;
                ddVeteranLevel.SelectedIndex = -1;
                ddHouse.SelectedIndex = -1;
                tbPriority.Text = string.Empty;
                tbMax.Text = string.Empty;
                tbTechLevel.Text = string.Empty;
                tbGroup.Text = string.Empty;
                tbWaypoint.Text = string.Empty;

                selTaskForce.Text = string.Empty;
                selTaskForce.Tag = null;

                selScript.Text = string.Empty;
                selScript.Tag = null;

                selTag.Text = string.Empty;
                selTag.Tag = null;

                checkBoxes.ForEach(chk => chk.Checked = false);

                return;
            }

            tbName.Text = editedTeamType.Name;
            ddVeteranLevel.SelectedIndex = editedTeamType.VeteranLevel;
            ddHouse.SelectedIndex = ddHouse.Items.FindIndex(i => i.Text == (editedTeamType.House == null ? "" : editedTeamType.House.ININame));
            tbPriority.Value = editedTeamType.Priority;
            tbMax.Value = editedTeamType.Max;
            tbTechLevel.Value = editedTeamType.TechLevel;
            tbGroup.Value = editedTeamType.Group;
            tbWaypoint.Value = editedTeamType.Waypoint;

            if (editedTeamType.TaskForce != null)
                selTaskForce.Text = editedTeamType.TaskForce.Name + " (" + editedTeamType.TaskForce.ININame + ")";
            else
                selTaskForce.Text = string.Empty;

            if (editedTeamType.Script != null)
                selScript.Text = editedTeamType.Script.Name + " (" + editedTeamType.Script.Name + ")";
            else
                selScript.Text = string.Empty;

            if (editedTeamType.Tag != null)
                selTag.Text = editedTeamType.Tag.Name + " (" + editedTeamType.Tag.ID + ")";
            else
                selTag.Text = string.Empty;
        }
    }
}
