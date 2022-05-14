using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rampastring.XNAUI;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class MegamapClickedEventArgs : EventArgs
    {
        public MegamapClickedEventArgs(Point2D clickedPoint)
        {
            ClickedPoint = clickedPoint;
        }

        /// <summary>
        /// The pixel point on the entire map that was clicked on.
        /// </summary>
        public Point2D ClickedPoint { get; }
    }

    public class MegamapWindow : EditorWindow
    {
        public MegamapWindow(WindowManager windowManager, Texture2D megamapTexture, bool enableToolbar) : base(windowManager)
        {
            this.enableToolbar = enableToolbar;
            this.megamapTexture = megamapTexture;
            ClientRectangleUpdated += MegamapWindow_ClientRectangleUpdated;
        }

        public event EventHandler<MegamapClickedEventArgs> MegamapClicked;

        public Rectangle CameraRectangle { get; set; }

        private void MegamapWindow_ClientRectangleUpdated(object sender, EventArgs e)
        {
            int startY;
            int previewHeight;

            if (enableToolbar)
            {
                startY = Constants.UIButtonHeight;
                previewHeight = Height - Constants.UIButtonHeight;
            }
            else
            {
                startY = 0;
                previewHeight = Height;
            }

            float xRatio = Width / (float)megamapTexture.Width;
            float yRatio = previewHeight / (float)megamapTexture.Height;
            float drawRatio = Math.Min(xRatio, yRatio);

            int drawWidth = (int)(megamapTexture.Width * drawRatio);
            int drawHeight = (int)(megamapTexture.Height * drawRatio);

            textureDrawRectangle = new Rectangle((Width - drawWidth) / 2, startY + (previewHeight - drawHeight) / 2, drawWidth, drawHeight);

            if (closeButton != null)
                closeButton.X = Width - closeButton.Width;
        }

        private bool enableToolbar;

        private Texture2D megamapTexture;

        private EditorButton closeButton;

        private Rectangle textureDrawRectangle;

        private bool wasLeftDown = false;
        private Point2D oldWindowPosition;

        public override void Initialize()
        {
            if (enableToolbar)
            {
                closeButton = new EditorButton(WindowManager);
                closeButton.Name = "btnCloseX";
                closeButton.Width = Constants.UIButtonHeight;
                closeButton.Height = Constants.UIButtonHeight;
                closeButton.Text = "X";
                closeButton.X = Width - closeButton.Width;
                closeButton.Y = 0;
                AddChild(closeButton);
                closeButton.LeftClick += (s, e) => Hide();

                var btnIncreaseSize = new EditorButton(WindowManager);
                btnIncreaseSize.Name = nameof(btnIncreaseSize);
                btnIncreaseSize.Width = Constants.UIButtonHeight;
                btnIncreaseSize.Height = Constants.UIButtonHeight;
                btnIncreaseSize.Text = "+";
                btnIncreaseSize.X = 0;
                btnIncreaseSize.Y = 0;
                AddChild(btnIncreaseSize);
                btnIncreaseSize.LeftClick += (s, e) => { if (Width < WindowManager.RenderResolutionX) Width += 20; if (Height < WindowManager.RenderResolutionY) Height += 10; };

                var btnDecreaseSize = new EditorButton(WindowManager);
                btnDecreaseSize.Name = nameof(btnDecreaseSize);
                btnDecreaseSize.Width = Constants.UIButtonHeight;
                btnDecreaseSize.Height = Constants.UIButtonHeight;
                btnDecreaseSize.Text = "-";
                btnDecreaseSize.X = btnIncreaseSize.Right;
                btnDecreaseSize.Y = 0;
                AddChild(btnDecreaseSize);
                btnDecreaseSize.LeftClick += (s, e) => { if (Width > 100) Width -= 20; if (Height > 100) Height -= 10; };
            }

            Name = nameof(MegamapWindow);
            base.Initialize();
        }

        public void Open()
        {
            Show();
        }

        private void MoveCamera()
        {
            var cursorPoint = GetCursorPoint();

            double x = cursorPoint.X - textureDrawRectangle.X;
            x /= textureDrawRectangle.Width;
            x *= megamapTexture.Width;

            double y = cursorPoint.Y - textureDrawRectangle.Y;
            y /= textureDrawRectangle.Height;
            y *= megamapTexture.Height;

            MegamapClicked?.Invoke(this, new MegamapClickedEventArgs(new Point2D((int)x, (int)y)));
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.IsKeyHeldDown(Keys.Escape))
                WindowManager.RemoveControl(this);

            if (!IsChildActive)
            {
                if (IsActive && Cursor.LeftDown)
                {
                    if (!wasLeftDown)
                        oldWindowPosition = new Point2D(X, Y);

                    CanBeMoved = !Keyboard.IsShiftHeldDown();
                    if (!CanBeMoved)
                        MoveCamera();

                    wasLeftDown = true;
                }
                else if (wasLeftDown)
                {
                    if (oldWindowPosition == new Point2D(X, Y))
                    {
                        MoveCamera();
                    }

                    wasLeftDown = false;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            DrawPanel();

            DrawTexture(megamapTexture, textureDrawRectangle, Color.White);
            
            if (CameraRectangle.Width > 0 && CameraRectangle.Height > 0)
            {
                double xPos = (CameraRectangle.X / (double)megamapTexture.Width) * textureDrawRectangle.Width;
                double yPos = (CameraRectangle.Y / (double)megamapTexture.Height) * textureDrawRectangle.Height;
                double width = (CameraRectangle.Width / (double)megamapTexture.Width) * textureDrawRectangle.Width;
                double height = (CameraRectangle.Height / (double)megamapTexture.Height) * textureDrawRectangle.Height;

                DrawRectangle(new Rectangle(textureDrawRectangle.X + (int)xPos, textureDrawRectangle.Y + (int)yPos, (int)width, (int)height), Color.White, 1);
            }

            DrawChildren(gameTime);

            if (!enableToolbar)
            {
                DrawStringWithShadow("Press ESC to close", 1, new Vector2(Constants.UIEmptySideSpace, Constants.UIEmptyTopSpace), Color.Red, 1.0f);
            }

            DrawPanelBorders();
        }
    }
}
