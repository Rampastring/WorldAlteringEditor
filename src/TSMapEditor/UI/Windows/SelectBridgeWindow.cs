using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class SelectBridgeWindow : SelectObjectWindow<BridgeType>
    {
        public SelectBridgeWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;
        public bool Success = false;

        public override void Initialize()
        {
            Name = nameof(SelectBridgeWindow);
            base.Initialize();

            FindChild<EditorButton>("btnSelect").LeftClick += BtnSelect_LeftClick;
        }

        protected override void LbObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbObjectList.SelectedItem == null)
            {
                SelectedObject = null;
                return;
            }

            SelectedObject = (BridgeType)lbObjectList.SelectedItem.Tag;
        }

        protected void BtnSelect_LeftClick(object sender, EventArgs e)
        {
            Success = true;
        }

        public void Open()
        {
            Success = false;
            Open(null);
        }

        protected override void ListObjects()
        {
            lbObjectList.Clear();

            foreach (BridgeType bridge in map.EditorConfig.Bridges)
            {
                lbObjectList.AddItem(new XNAListBoxItem() { Text = bridge.Name, Tag = bridge });
            }
        }
    }
}