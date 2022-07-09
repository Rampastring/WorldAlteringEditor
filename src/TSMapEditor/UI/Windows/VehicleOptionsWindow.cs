using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to edit the properties of a vehicle.
    /// </summary>
    public class VehicleOptionsWindow : INItializableWindow
    {
        public VehicleOptionsWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorNumberTextBox tbStrength;
        private XNADropDown ddMission;
        private XNADropDown ddVeterancy;
        private EditorNumberTextBox tbGroup;
        private EditorNumberTextBox tbFollowsID;
        private XNACheckBox chkOnBridge;
        private XNACheckBox chkAutocreateNoRecruitable;
        private XNACheckBox chkAutocreateYesRecruitable;
        private EditorPopUpSelector attachedTagSelector;

        private Unit unit;

        private SelectTagWindow selectTagWindow;

        public override void Initialize()
        {
            Name = nameof(VehicleOptionsWindow);
            base.Initialize();

            tbStrength = FindChild<EditorNumberTextBox>(nameof(tbStrength));
            ddMission = FindChild<XNADropDown>(nameof(ddMission));
            ddVeterancy = FindChild<XNADropDown>(nameof(ddVeterancy));
            tbGroup = FindChild<EditorNumberTextBox>(nameof(tbGroup));
            tbFollowsID = FindChild<EditorNumberTextBox>(nameof(tbFollowsID));
            chkOnBridge = FindChild<XNACheckBox>(nameof(chkOnBridge));
            chkAutocreateNoRecruitable = FindChild<XNACheckBox>(nameof(chkAutocreateNoRecruitable));
            chkAutocreateYesRecruitable = FindChild<XNACheckBox>(nameof(chkAutocreateYesRecruitable));
            attachedTagSelector = FindChild<EditorPopUpSelector>(nameof(attachedTagSelector));

            attachedTagSelector.LeftClick += AttachedTagSelector_LeftClick;

            selectTagWindow = new SelectTagWindow(WindowManager, map);
            var tagDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectTagWindow);
            tagDarkeningPanel.Hidden += (s, e) => SelectionWindow_ApplyEffect(w => unit.AttachedTag = w.SelectedObject, selectTagWindow);

            FindChild<EditorButton>("btnOK").LeftClick += BtnOK_LeftClick;
        }

        private void SelectionWindow_ApplyEffect<T>(Action<T> action, T window)
        {
            action(window);
            RefreshValues();
        }

        private void AttachedTagSelector_LeftClick(object sender, EventArgs e)
        {
            selectTagWindow.Open(unit.AttachedTag);
        }

        public void Open(Unit unit)
        {
            this.unit = unit;
            RefreshValues();
            Show();
        }

        private void RefreshValues()
        {
            tbStrength.Value = unit.HP;
            ddMission.SelectedIndex = ddMission.Items.FindIndex(item => item.Text == unit.Mission);
            ddVeterancy.SelectedIndex = unit.Veterancy;
            tbGroup.Value = unit.Group;
            tbFollowsID.Value = unit.FollowsID;
            chkOnBridge.Checked = unit.High;
            chkAutocreateNoRecruitable.Checked = unit.AutocreateNoRecruitable;
            chkAutocreateYesRecruitable.Checked = unit.AutocreateYesRecruitable;
            attachedTagSelector.Text = unit.AttachedTag == null ? string.Empty : unit.AttachedTag.GetDisplayString();
            attachedTagSelector.Tag = unit.AttachedTag;
        }

        private void BtnOK_LeftClick(object sender, EventArgs e)
        {
            unit.HP = Math.Min(Constants.ObjectHealthMax, Math.Max(tbStrength.Value, 0));
            unit.Mission = ddMission.SelectedItem == null ? unit.Mission : ddMission.SelectedItem.Text;
            unit.Veterancy = ddVeterancy.SelectedIndex;
            unit.Group = tbGroup.Value;
            unit.FollowsID = tbFollowsID.Value;

            if (unit.FollowsID > -1 && unit.FollowsID < map.Units.Count)
                unit.FollowedUnit = map.Units[unit.FollowsID];
            else
                unit.FollowedUnit = null;

            unit.High = chkOnBridge.Checked;
            unit.AutocreateNoRecruitable = chkAutocreateNoRecruitable.Checked;
            unit.AutocreateYesRecruitable = chkAutocreateYesRecruitable.Checked;
            unit.AttachedTag = (Tag)attachedTagSelector.Tag;

            Hide();
        }
    }
}
