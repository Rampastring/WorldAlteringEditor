using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows.MainMenuWindows
{
    public class CreateNewMapWindow : INItializableWindow
    {
        private const int MinMapSize = 50;
        private const int MaxMapSize = 512;

        public CreateNewMapWindow(WindowManager windowManager, string gameDirectory) : base(windowManager)
        {
            this.gameDirectory = gameDirectory;
        }

        private readonly string gameDirectory;

        private XNADropDown ddTheater;
        private EditorNumberTextBox tbWidth;
        private EditorNumberTextBox tbHeight;

        public override void Initialize()
        {
            Name = nameof(CreateNewMapWindow);
            base.Initialize();

            ddTheater = FindChild<XNADropDown>(nameof(ddTheater));
            tbWidth = FindChild<EditorNumberTextBox>(nameof(tbWidth));
            tbHeight = FindChild<EditorNumberTextBox>(nameof(tbHeight));

            FindChild<EditorButton>("btnCreate").LeftClick += BtnCreate_LeftClick;

            ddTheater.SelectedIndex = 0;

            CenterOnParent();
        }

        private void BtnCreate_LeftClick(object sender, EventArgs e)
        {
            if (tbWidth.Value < MinMapSize || tbHeight.Value < MinMapSize || tbWidth.Value + tbHeight.Value >= MaxMapSize)
                return;

            MapSetup.InitializeMap(WindowManager, gameDirectory, true, null, ddTheater.SelectedItem.Text, new Point2D(tbWidth.Value, tbHeight.Value));
            WindowManager.RemoveControl(this);
        }
    }
}
