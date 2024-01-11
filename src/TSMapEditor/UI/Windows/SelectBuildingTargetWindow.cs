using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using System;
using TSMapEditor.Models;
using TSMapEditor.CCEngine;
using Microsoft.Xna.Framework;

namespace TSMapEditor.UI.Windows
{
    public class SelectBuildingTargetWindow : SelectObjectWindow<int>
    {
        public SelectBuildingTargetWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public override void Initialize()
        {
            Name = nameof(SelectBuildingTargetWindow);
            base.Initialize();
        }

        protected override void LbObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbObjectList.SelectedItem?.Tag == null)
            {
                SelectedObject = -1;
                return;
            }

            SelectedObject = (int)lbObjectList.SelectedItem.Tag;
        }

        protected override void ListObjects()
        {
            lbObjectList.Clear();

            bool useDifferentColor = false;

            foreach (BuildingType buildingType in map.Rules.BuildingTypes)
            {
                AddItem(buildingType, "Least threat", (int)BuildingWithPropertyType.LeastThreat, useDifferentColor);
                AddItem(buildingType, "Highest threat", (int)BuildingWithPropertyType.HighestThreat, useDifferentColor);
                AddItem(buildingType, "Nearest", (int)BuildingWithPropertyType.Nearest, useDifferentColor);
                AddItem(buildingType, "Farthest", (int)BuildingWithPropertyType.Farthest, useDifferentColor);

                useDifferentColor = !useDifferentColor;
            }
        }

        private void AddItem(BuildingType buildingType, string description, int targetTypeNumber, bool useDifferentColor)
        {
            Color color = useDifferentColor ? lbObjectList.DefaultItemColor * 0.7f : lbObjectList.DefaultItemColor;

            int number = buildingType.Index + targetTypeNumber;
            lbObjectList.AddItem(new XNAListBoxItem() { Text = $"{number} {buildingType.GetEditorDisplayName()} ({description})", Tag = number, TextColor = color });

            if (SelectedObject == number)
                lbObjectList.SelectedIndex = lbObjectList.Items.Count - 1;
        }
    }
}
