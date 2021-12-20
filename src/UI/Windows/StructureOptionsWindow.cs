using System;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to edit the properties of a building.
    /// </summary>
    public class StructureOptionsWindow : INItializableWindow
    {
        public StructureOptionsWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorNumberTextBox tbStrength;
        private XNACheckBox chkSellable;
        private XNACheckBox chkRebuild;
        private XNACheckBox chkPowered;
        private XNACheckBox chkAIRepairable;
        private XNACheckBox chkNominal;
        private XNADropDown ddSpotlight;
        private EditorPopUpSelector upgrade1Selector;
        private EditorPopUpSelector upgrade2Selector;
        private EditorPopUpSelector upgrade3Selector;
        private EditorPopUpSelector attachedTagSelector;

        private Structure structure;

        private SelectTagWindow selectTagWindow;

        private EditorButton btnOK;

        public override void Initialize()
        {
            Name = nameof(StructureOptionsWindow);
            base.Initialize();

            tbStrength = FindChild<EditorNumberTextBox>(nameof(tbStrength));
            chkSellable = FindChild<XNACheckBox>(nameof(chkSellable));
            chkRebuild = FindChild<XNACheckBox>(nameof(chkRebuild));
            chkPowered = FindChild<XNACheckBox>(nameof(chkPowered));
            chkAIRepairable = FindChild<XNACheckBox>(nameof(chkAIRepairable));
            chkNominal = FindChild<XNACheckBox>(nameof(chkNominal));
            ddSpotlight = FindChild<XNADropDown>(nameof(ddSpotlight));
            upgrade1Selector = FindChild<EditorPopUpSelector>(nameof(upgrade1Selector));
            upgrade2Selector = FindChild<EditorPopUpSelector>(nameof(upgrade2Selector));
            upgrade3Selector = FindChild<EditorPopUpSelector>(nameof(upgrade3Selector));
            attachedTagSelector = FindChild<EditorPopUpSelector>(nameof(attachedTagSelector));

            attachedTagSelector.LeftClick += AttachedTagSelector_LeftClick;

            selectTagWindow = new SelectTagWindow(WindowManager, map);
            var tagDarkeningPanel = DarkeningPanel.InitializeAndAddToParentControlWithChild(WindowManager, Parent, selectTagWindow);
            tagDarkeningPanel.Hidden += (s, e) => SelectionWindow_ApplyEffect(w => structure.AttachedTag = w.SelectedObject, selectTagWindow);

            FindChild<EditorButton>("btnOK").LeftClick += BtnOK_LeftClick;
        }

        private void SelectionWindow_ApplyEffect<T>(Action<T> action, T window)
        {
            action(window);
            RefreshValues();
        }

        private void AttachedTagSelector_LeftClick(object sender, EventArgs e)
        {
            selectTagWindow.Open(structure.AttachedTag);
        }

        public void Open(Structure structure)
        {
            this.structure = structure;
            RefreshValues();
            Show();
        }

        private void RefreshValues()
        {
            tbStrength.Value = structure.HP;
            chkSellable.Checked = structure.AISellable;
            chkRebuild.Checked = structure.AIRebuildable;
            chkPowered.Checked = structure.Powered;
            chkAIRepairable.Checked = structure.AIRepairable;
            chkNominal.Checked = structure.Nominal;
            ddSpotlight.SelectedIndex = (int)structure.Spotlight;
            FetchUpgrade(upgrade1Selector, 0);
            FetchUpgrade(upgrade2Selector, 1);
            FetchUpgrade(upgrade3Selector, 2);
            attachedTagSelector.Text = structure.AttachedTag == null ? string.Empty : structure.AttachedTag.Name + " (" + structure.AttachedTag.ID + ")";
            attachedTagSelector.Tag = structure.AttachedTag;
        }

        private void FetchUpgrade(EditorPopUpSelector upgradeSelector, int upgradeIndex)
        {
            var upgrade = structure.Upgrades[upgradeIndex];
            upgradeSelector.Tag = upgrade;

            if (upgrade == null)
                upgradeSelector.Text = string.Empty;
            else
                upgradeSelector.Text = upgrade.Name + " (" + upgrade.ININame + ")";
        }

        private void BtnOK_LeftClick(object sender, EventArgs e)
        {
            structure.HP = Math.Min(tbStrength.Value, Constants.ObjectHealthMax);
            structure.AISellable = chkSellable.Checked;
            structure.AIRebuildable = chkRebuild.Checked;
            structure.Powered = chkPowered.Checked;
            structure.AIRepairable = chkAIRepairable.Checked;
            structure.Nominal = chkNominal.Checked;
            structure.Spotlight = (SpotlightType)ddSpotlight.SelectedIndex;
            structure.Upgrades[0] = (BuildingType)upgrade1Selector.Tag;
            structure.Upgrades[1] = (BuildingType)upgrade2Selector.Tag;
            structure.Upgrades[2] = (BuildingType)upgrade3Selector.Tag;
            structure.AttachedTag = (Tag)attachedTagSelector.Tag;

            Hide();
        }
    }
}
