using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.Controls
{
    public class EditorDescriptionPanel : EditorPanel
    {
        public EditorDescriptionPanel(WindowManager windowManager) : base(windowManager)
        {
        }

        private XNALabel lblDescription;

        public override string Text
        {
            get => lblDescription.Text;
            set => lblDescription.Text = Renderer.FixText(value, lblDescription.FontIndex, Width - (Constants.UIEmptySideSpace * 2)).Text;
        }

        public override void Initialize()
        {
            lblDescription = new XNALabel(WindowManager);
            lblDescription.Name = Name + "." + nameof(lblDescription);
            lblDescription.X = Constants.UIEmptySideSpace;
            lblDescription.Y = Constants.UIEmptyTopSpace;
            lblDescription.Text = string.Empty;
            AddChild(lblDescription);

            base.Initialize();
        }
    }
}
