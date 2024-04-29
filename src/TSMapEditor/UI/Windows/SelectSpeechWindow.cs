using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using System;
using TSMapEditor.Models;

namespace TSMapEditor.UI.Windows
{
    public class SelectSpeechWindow : SelectObjectWindow<int>
    {
        public SelectSpeechWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public override void Initialize()
        {
            Name = nameof(SelectSpeechWindow);
            base.Initialize();
        }

        protected override void LbObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbObjectList.SelectedItem == null || lbObjectList.SelectedItem.Tag == null)
            {
                SelectedObject = -1;
                return;
            }

            SelectedObject = (int)lbObjectList.SelectedItem.Tag;
        }

        protected override void ListObjects()
        {
            lbObjectList.Clear();

            foreach (var kvp in map.EditorConfig.Speeches)
            {
                lbObjectList.AddItem(new XNAListBoxItem() { Text = $"{kvp.Key} {kvp.Value}", Tag = kvp.Key });
                if (kvp.Key == SelectedObject)
                    lbObjectList.SelectedIndex = lbObjectList.Items.Count - 1;
            }
        }
    }
}
