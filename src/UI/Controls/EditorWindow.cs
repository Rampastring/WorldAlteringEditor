using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;

namespace TSMapEditor.UI.Controls
{
    public class EditorWindow : EditorPanel
    {
        private const float AppearingRate = 0.9f;
        private const float DisappearingRate = -1.0f;

        public EditorWindow(WindowManager windowManager) : base(windowManager)
        {
            DrawMode = ControlDrawMode.UNIQUE_RENDER_TARGET;
        }

        public event EventHandler Closed;

        protected bool CanBeMoved { get; private set; } = true;

        private bool isDragged;
        private Point lastCursorPoint;

        private void CloseButton_LeftClick(object sender, EventArgs e)
        {
            Hide();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public override void ParseAttributeFromINI(IniFile iniFile, string key, string value)
        {
            if (key == nameof(CanBeMoved))
            {
                CanBeMoved = Conversions.BooleanFromString(value, CanBeMoved);
            }

            base.ParseAttributeFromINI(iniFile, key, value);
        }

        public void Hide()
        {
            AlphaRate = DisappearingRate;
        }

        protected virtual void Show()
        {
            AlphaRate = AppearingRate;
            Alpha = 0f;
            Enable();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Alpha <= 0f && AlphaRate < 0.0f)
                Disable();

            if (IsActive && CanBeMoved &&
                !(WindowManager.SelectedControl is XNAScrollBar) &&
                !(WindowManager.SelectedControl is XNATrackbar))
            {
                if (Cursor.LeftPressedDown)
                {
                    isDragged = true;
                    lastCursorPoint = GetCursorPoint();
                }
                else
                {
                    isDragged = Cursor.LeftDown;

                    if (isDragged)
                    {
                        Point newCursorPoint = GetCursorPoint();
                        X = X + newCursorPoint.X - lastCursorPoint.X;
                        Y = Y + newCursorPoint.Y - lastCursorPoint.Y;
                        if (X + ScaledWidth > WindowManager.RenderResolutionX)
                            X = WindowManager.RenderResolutionX - ScaledWidth;
                        else if (X < 0)
                            X = 0;

                        if (Y + ScaledHeight > WindowManager.RenderResolutionY)
                            Y = WindowManager.RenderResolutionY - ScaledHeight;
                        else if (Y < 0)
                            Y = 0;
                    }

                    lastCursorPoint = GetCursorPoint();
                }
            }
            else
            {
                isDragged = false;
            }
        }
    }
}
