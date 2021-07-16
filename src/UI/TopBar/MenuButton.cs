using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;

namespace TSMapEditor.UI.TopBar
{
    /// <summary>
    /// A button that opens a menu.
    /// </summary>
    public class MenuButton : XNAButton
    {
        public MenuButton(WindowManager windowManager, XNAContextMenu contextMenu) : base(windowManager)
        {
            ContextMenu = contextMenu;

            Height = Constants.UITopBarMenuHeight;
            FontIndex = Constants.UIBoldFont;
        }


        public XNAContextMenu ContextMenu { get; }

        /*
         * Warning: this class pretty much hacks around the typical behaviour 
         * of context menus to allow them to convincingly work as menus.
         * Firstly, the menu should open when the primary mouse button is
         * pressed down on this button. 
         * Secondly, clicking on this button (iow. releasing the mouse button)
         * should only close the context menu if it wasn't opened when the button
         * was first pressed down.
         */

        private bool contextMenuDisabledOnCurrentFrame = false;
        private bool contextMenuOpenedOnClick = false;

        public override void Initialize()
        {
            base.Initialize();

            Width = (int)Renderer.GetTextDimensions(Text, FontIndex).X + Constants.UIEmptySideSpace * 2;

            AddChild(ContextMenu);
            ContextMenu.EnabledChanged += ContextMenu_EnabledChanged;
        }

        private void ContextMenu_EnabledChanged(object sender, System.EventArgs e)
        {
            if (!ContextMenu.Enabled)
            {
                RemapColor = UISettings.ActiveSettings.AltColor;
                TextColorIdle = UISettings.ActiveSettings.ButtonTextColor;
                contextMenuDisabledOnCurrentFrame = true;
            }
        }

        public override void OnMouseLeftDown()
        {
            base.OnMouseLeftDown();

            if (!ContextMenu.Enabled)
            {
                OpenContextMenu();
                contextMenuOpenedOnClick = true;
            }            
        }

        public override void OnLeftClick()
        {
            base.OnLeftClick();

            if ((!ContextMenu.Enabled && !contextMenuDisabledOnCurrentFrame) || contextMenuOpenedOnClick)
                OpenContextMenu();
            else
                ContextMenu.Disable();

            contextMenuOpenedOnClick = false;
        }

        public void OpenContextMenu()
        {
            if (!ContextMenu.Enabled)
            {
                ContextMenu.Open(new Point(0, Bottom));
                RemapColor = UISettings.ActiveSettings.FocusColor;
                TextColorIdle = UISettings.ActiveSettings.ButtonHoverColor;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            contextMenuDisabledOnCurrentFrame = false;
        }
    }
}
