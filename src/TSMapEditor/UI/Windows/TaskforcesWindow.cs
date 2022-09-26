using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to edit the map's TaskForces.
    /// </summary>
    public class TaskforcesWindow : INItializableWindow
    {
        public TaskforcesWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorListBox lbTaskForces;
        private EditorTextBox tbTaskForceName;
        private EditorNumberTextBox tbGroup;
        private EditorListBox lbUnitEntries;
        private EditorNumberTextBox tbUnitCount;
        private EditorSuggestionTextBox tbSearchUnit;
        private EditorListBox lbUnitType;

        private TaskForce editedTaskForce;

        public override void Initialize()
        {
            Name = nameof(TaskforcesWindow);

            base.Initialize();

            lbTaskForces = FindChild<EditorListBox>(nameof(lbTaskForces));
            tbTaskForceName = FindChild<EditorTextBox>(nameof(tbTaskForceName));
            tbGroup = FindChild<EditorNumberTextBox>(nameof(tbGroup));
            lbUnitEntries = FindChild<EditorListBox>(nameof(lbUnitEntries));
            tbUnitCount = FindChild<EditorNumberTextBox>(nameof(tbUnitCount));
            tbSearchUnit = FindChild<EditorSuggestionTextBox>(nameof(tbSearchUnit));
            UIHelpers.AddSearchTipsBoxToControl(tbSearchUnit);

            lbUnitType = FindChild<EditorListBox>(nameof(lbUnitType));

            var btnNewTaskForce = FindChild<EditorButton>("btnNewTaskForce");
            btnNewTaskForce.LeftClick += BtnNewTaskForce_LeftClick;

            var btnDeleteTaskForce = FindChild<EditorButton>("btnDeleteTaskForce");
            btnDeleteTaskForce.LeftClick += BtnDeleteTaskForce_LeftClick;

            var btnCloneTaskForce = FindChild<EditorButton>("btnCloneTaskForce");
            btnCloneTaskForce.LeftClick += BtnCloneTaskForce_LeftClick;

            var btnAddUnit = FindChild<EditorButton>("btnAddUnit");
            btnAddUnit.LeftClick += BtnAddUnit_LeftClick;

            var btnDeleteUnit = FindChild<EditorButton>("btnDeleteUnit");
            btnDeleteUnit.LeftClick += BtnDeleteUnit_LeftClick;

            ListUnits();

            lbTaskForces.SelectedIndexChanged += LbTaskForces_SelectedIndexChanged;
            lbUnitEntries.SelectedIndexChanged += LbUnitEntries_SelectedIndexChanged;

            tbSearchUnit.TextChanged += TbSearchUnit_TextChanged;
            tbSearchUnit.EnterPressed += TbSearchUnit_EnterPressed;

            lbUnitType.SelectedIndexChanged += LbUnitType_SelectedIndexChanged;
            tbUnitCount.TextChanged += TbUnitCount_TextChanged;

            tbTaskForceName.TextChanged += TbTaskForceName_TextChanged;
            tbGroup.TextChanged += TbGroup_TextChanged;
        }

        private void BtnDeleteUnit_LeftClick(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null || lbUnitEntries.SelectedItem == null)
                return;

            editedTaskForce.RemoveTechnoEntry(lbUnitEntries.SelectedIndex);
            EditTaskForce(editedTaskForce);
        }

        private void BtnAddUnit_LeftClick(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null)
                return;

            if (editedTaskForce.TechnoTypes[TaskForce.MaxTechnoCount - 1] != null)
                return;

            editedTaskForce.AddTechnoEntry(
                new TaskForceTechnoEntry() 
                { 
                    Count = 1, 
                    TechnoType = (TechnoType)lbUnitType.Items[0].Tag 
                });

            EditTaskForce(editedTaskForce);
            lbUnitEntries.SelectedIndex = lbUnitEntries.Items.Count - 1;
        }

        private void BtnCloneTaskForce_LeftClick(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null)
                return;

            map.TaskForces.Add(editedTaskForce.Clone(map.GetNewUniqueInternalId()));
            AddTaskForceToList(map.TaskForces[map.TaskForces.Count - 1]);
            SelectLastTaskForce();
        }

        private void BtnDeleteTaskForce_LeftClick(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null)
                return;

            if (Keyboard.IsKeyHeldDown(KeyboardCommands.Instance.SkipConfirmationKey))
            {
                DeleteTaskForce();
            }
            else
            {
                var messageBox = EditorMessageBox.Show(WindowManager,
                    "Confirm",
                    $"Are you sure you wish to delete '{editedTaskForce.Name}'?\r\n\r\n" +
                    $"You'll need to manually fix any TeamTypes using the TaskForce.",
                    MessageBoxButtons.YesNo);
                messageBox.YesClickedAction = _ => DeleteTaskForce();
            }
        }

        private void DeleteTaskForce()
        {
            map.TaskForces.Remove(editedTaskForce);
            map.TeamTypes.ForEach(tt =>
            {
                if (tt.TaskForce == editedTaskForce)
                    tt.TaskForce = null;
            });
            ListTaskForces();
            lbTaskForces.SelectedIndex = -1;
        }

        private void BtnNewTaskForce_LeftClick(object sender, System.EventArgs e)
        {
            map.TaskForces.Add(new TaskForce(map.GetNewUniqueInternalId()) { Name = "New TaskForce" });
            AddTaskForceToList(map.TaskForces[map.TaskForces.Count - 1]);
            SelectLastTaskForce();
        }

        private void SelectLastTaskForce()
        {
            lbTaskForces.SelectedIndex = lbTaskForces.Items.Count - 1;
            lbTaskForces.ScrollToBottom();
        }

        private void TbGroup_TextChanged(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null)
                return;

            editedTaskForce.Group = tbGroup.Value;
        }

        private void TbTaskForceName_TextChanged(object sender, System.EventArgs e)
        {
            if (editedTaskForce == null)
                return;

            editedTaskForce.Name = tbTaskForceName.Text;
            if (lbTaskForces.SelectedItem != null)
            {
                lbTaskForces.SelectedItem.Text = editedTaskForce.Name;
            }
        }

        private void LbUnitType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (lbUnitType.SelectedItem == null)
                return;

            var unitEntry = lbUnitEntries.SelectedItem;
            if (unitEntry == null)
            {
                return;
            }

            var taskForceTechno = editedTaskForce.TechnoTypes[lbUnitEntries.SelectedIndex];
            taskForceTechno.TechnoType = (TechnoType)lbUnitType.SelectedItem.Tag;
            unitEntry.Text = GetUnitEntryText(taskForceTechno);
        }

        private void TbUnitCount_TextChanged(object sender, System.EventArgs e)
        {
            var unitEntry = lbUnitEntries.SelectedItem;
            if (unitEntry == null)
            {
                return;
            }

            var taskForceTechno = editedTaskForce.TechnoTypes[lbUnitEntries.SelectedIndex];
            taskForceTechno.Count = tbUnitCount.Value;
            unitEntry.Text = GetUnitEntryText(taskForceTechno);
        }

        private void LbUnitEntries_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var unitEntry = lbUnitEntries.SelectedItem;
            if (unitEntry == null)
            {
                tbUnitCount.Text = string.Empty;
                return;
            }

            var taskForceTechno = editedTaskForce.TechnoTypes[lbUnitEntries.SelectedIndex];

            lbUnitType.SelectedIndexChanged -= LbUnitType_SelectedIndexChanged;
            lbUnitType.SelectedIndex = lbUnitType.Items.FindIndex(u => ((TechnoType)u.Tag) == taskForceTechno.TechnoType);
            lbUnitType.ViewTop = lbUnitType.SelectedIndex * lbUnitType.LineHeight;
            lbUnitType.SelectedIndexChanged += LbUnitType_SelectedIndexChanged;

            tbUnitCount.TextChanged -= TbUnitCount_TextChanged;
            tbUnitCount.Value = taskForceTechno.Count;
            tbUnitCount.TextChanged += TbUnitCount_TextChanged;
        }

        private void LbTaskForces_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var selectedItem = lbTaskForces.SelectedItem;
            if (selectedItem == null)
            {
                EditTaskForce(null);
                return;
            }

            EditTaskForce((TaskForce)selectedItem.Tag);
        }

        private void TbSearchUnit_EnterPressed(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearchUnit.Text) || tbSearchUnit.Text == tbSearchUnit.Suggestion)
                return;

            FindNextMatchingUnit();
        }

        private void TbSearchUnit_TextChanged(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearchUnit.Text) || tbSearchUnit.Text == tbSearchUnit.Suggestion)
                return;

            lbUnitType.SelectedIndex = -1;
            FindNextMatchingUnit();
        }

        private void FindNextMatchingUnit()
        {
            for (int i = lbUnitType.SelectedIndex + 1; i < lbUnitType.Items.Count; i++)
            {
                var gameObjectType = (TechnoType)lbUnitType.Items[i].Tag;

                if (gameObjectType.ININame.ToUpperInvariant().Contains(tbSearchUnit.Text.ToUpperInvariant()) ||
                    gameObjectType.GetEditorDisplayName().ToUpperInvariant().Contains(tbSearchUnit.Text.ToUpperInvariant()))
                {
                    lbUnitType.SelectedIndex = i;
                    lbUnitType.ViewTop = lbUnitType.SelectedIndex * lbUnitType.LineHeight;
                    break;
                }
            }
        }

        private void ListUnits()
        {
            var gameObjectTypeList = new List<GameObjectType>();
            gameObjectTypeList.AddRange(map.Rules.AircraftTypes);
            gameObjectTypeList.AddRange(map.Rules.InfantryTypes);
            gameObjectTypeList.AddRange(map.Rules.UnitTypes);
            gameObjectTypeList = gameObjectTypeList.OrderBy(g => g.ININame).ToList();

            foreach (GameObjectType objectType in gameObjectTypeList)
            {
                lbUnitType.AddItem(new XNAListBoxItem() { Text = objectType.ININame + " (" + objectType.GetEditorDisplayName() + ")", Tag = objectType });
            }
        }

        public void Open()
        {
            Show();
            ListTaskForces();
        }

        private void ListTaskForces()
        {
            lbTaskForces.SelectedIndexChanged -= LbTaskForces_SelectedIndexChanged;
            lbTaskForces.Clear();

            foreach (TaskForce taskForce in map.TaskForces)
            {
                AddTaskForceToList(taskForce);
            }

            lbTaskForces.SelectedIndexChanged += LbTaskForces_SelectedIndexChanged;
        }

        private void AddTaskForceToList(TaskForce taskForce)
        {
            lbTaskForces.AddItem(new XNAListBoxItem() { Text = taskForce.Name, Tag = taskForce });
        }

        private void EditTaskForce(TaskForce taskForce)
        {
            editedTaskForce = taskForce;

            if (taskForce == null)
            {
                tbTaskForceName.Text = string.Empty;
                tbGroup.Text = string.Empty;
                lbUnitEntries.Clear();
                tbUnitCount.Text = string.Empty;
                return;
            }

            tbSearchUnit.Text = tbSearchUnit.Suggestion;

            tbTaskForceName.Text = taskForce.Name;
            tbGroup.Value = taskForce.Group;

            lbUnitEntries.SelectedIndexChanged -= LbUnitEntries_SelectedIndexChanged;
            lbUnitEntries.SelectedIndex = -1;
            lbUnitEntries.Clear();

            for (int i = 0; i < taskForce.TechnoTypes.Length; i++)
            {
                var taskForceTechno = taskForce.TechnoTypes[i];
                if (taskForceTechno == null)
                    break;

                lbUnitEntries.AddItem(GetUnitEntryText(taskForceTechno));
            }

            lbUnitEntries.SelectedIndexChanged += LbUnitEntries_SelectedIndexChanged;

            if (lbUnitEntries.Items.Count > 0)
            {
                lbUnitEntries.SelectedIndex = 0;
            }
        }

        private string GetUnitEntryText(TaskForceTechnoEntry taskForceTechno)
        {
            return $"{taskForceTechno.Count} {taskForceTechno.TechnoType.ININame} ({taskForceTechno.TechnoType.GetEditorDisplayName()})";
        }
    }
}
