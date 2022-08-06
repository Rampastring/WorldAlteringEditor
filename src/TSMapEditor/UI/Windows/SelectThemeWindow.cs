using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using TSMapEditor.Models;

namespace TSMapEditor.UI.Windows
{
    public class SelectThemeWindow : SelectObjectWindow<Theme>
    {
        public SelectThemeWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public override void Initialize()
        {
            Name = nameof(SelectThemeWindow);
            base.Initialize();
        }

        protected override void LbObjectList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbObjectList.SelectedItem == null)
            {
                SelectedObject = null;
                return;
            }

            SelectedObject = (Theme)lbObjectList.SelectedItem.Tag;
        }

        protected override void ListObjects()
        {
            lbObjectList.Clear();

            List<Theme> themes = map.Rules.Themes.GetThemes();

            for (int i = 0; i < themes.Count; i++)
            {
                Theme theme = themes[i];

                lbObjectList.AddItem(new XNAListBoxItem() { Text = $"{i} {theme.Name}", Tag = theme });
                if (theme == SelectedObject)
                    lbObjectList.SelectedIndex = lbObjectList.Items.Count - 1;
            }
        }
    }
}
