using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class LightingSettingsWindow : INItializableWindow
    {
        public LightingSettingsWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        private EditorNumberTextBox tbAmbientNormal;
        private EditorNumberTextBox tbLevelNormal;
        private EditorNumberTextBox tbRedNormal;
        private EditorNumberTextBox tbGreenNormal;
        private EditorNumberTextBox tbBlueNormal;

        private EditorNumberTextBox tbAmbientIS;
        private EditorNumberTextBox tbLevelIS;
        private EditorNumberTextBox tbRedIS;
        private EditorNumberTextBox tbGreenIS;
        private EditorNumberTextBox tbBlueIS;

        public override void Initialize()
        {
            Name = nameof(LightingSettingsWindow);
            base.Initialize();

            tbAmbientNormal = FindChild<EditorNumberTextBox>(nameof(tbAmbientNormal));
            tbLevelNormal = FindChild<EditorNumberTextBox>(nameof(tbLevelNormal));
            tbRedNormal = FindChild<EditorNumberTextBox>(nameof(tbRedNormal));
            tbGreenNormal = FindChild<EditorNumberTextBox>(nameof(tbGreenNormal));
            tbBlueNormal = FindChild<EditorNumberTextBox>(nameof(tbBlueNormal));

            tbAmbientIS = FindChild<EditorNumberTextBox>(nameof(tbAmbientIS));
            tbLevelIS = FindChild<EditorNumberTextBox>(nameof(tbLevelIS));
            tbRedIS = FindChild<EditorNumberTextBox>(nameof(tbRedIS));
            tbGreenIS = FindChild<EditorNumberTextBox>(nameof(tbGreenIS));
            tbBlueIS = FindChild<EditorNumberTextBox>(nameof(tbBlueIS));

            FindChild<EditorButton>("btnApply").LeftClick += BtnApply_LeftClick; ;
        }

        public void Open()
        {
            const string format = "0.000";

            tbAmbientNormal.Text = map.Lighting.Ambient.ToString(format);
            tbLevelNormal.Text = map.Lighting.Level.ToString(format);
            tbRedNormal.Text = map.Lighting.Red.ToString(format);
            tbGreenNormal.Text = map.Lighting.Green.ToString(format);
            tbBlueNormal.Text = map.Lighting.Blue.ToString(format);

            tbAmbientIS.Text = map.Lighting.IonAmbient.ToString(format);
            tbLevelIS.Text = map.Lighting.IonLevel.ToString(format);
            tbRedIS.Text = map.Lighting.IonRed.ToString(format);
            tbGreenIS.Text = map.Lighting.IonGreen.ToString(format);
            tbBlueIS.Text = map.Lighting.IonBlue.ToString(format);

            Show();
        }

        private void BtnApply_LeftClick(object sender, System.EventArgs e)
        {
            map.Lighting.Ambient = tbAmbientNormal.DoubleValue;
            map.Lighting.Level = tbLevelNormal.DoubleValue;
            map.Lighting.Red = tbRedNormal.DoubleValue;
            map.Lighting.Green = tbGreenNormal.DoubleValue;
            map.Lighting.Blue = tbBlueNormal.DoubleValue;

            map.Lighting.IonAmbient = tbAmbientIS.DoubleValue;
            map.Lighting.IonLevel = tbLevelIS.DoubleValue;
            map.Lighting.IonRed = tbRedIS.DoubleValue;
            map.Lighting.IonGreen = tbGreenIS.DoubleValue;
            map.Lighting.IonBlue = tbBlueIS.DoubleValue;

            Hide();
        }
    }
}
