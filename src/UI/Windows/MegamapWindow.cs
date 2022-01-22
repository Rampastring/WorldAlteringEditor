using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class MegamapWindow : EditorWindow
    {
        public MegamapWindow(WindowManager windowManager, Texture2D megamapTexture) : base(windowManager)
        {
            this.megamapTexture = megamapTexture;
        }

        private Texture2D megamapTexture;

        private Rectangle textureDrawRectangle;

        public override void Initialize()
        {
            base.Initialize();
            Width = WindowManager.RenderResolutionX;
            Height = WindowManager.RenderResolutionY;

            float xRatio = Width / (float)megamapTexture.Width;
            float yRatio = Height / (float)megamapTexture.Height;
            float drawRatio = Math.Min(xRatio, yRatio);

            int drawWidth = (int)(megamapTexture.Width * drawRatio);
            int drawHeight = (int)(megamapTexture.Height * drawRatio);

            textureDrawRectangle = new Rectangle((Width - drawWidth) / 2, (Height - drawHeight) / 2, drawWidth, drawHeight);
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.IsKeyHeldDown(Keys.Escape))
                WindowManager.RemoveControl(this);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawTexture(megamapTexture, textureDrawRectangle, Color.White);
        }
    }
}
