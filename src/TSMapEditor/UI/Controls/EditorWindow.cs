﻿using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Settings;

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
        public event EventHandler InteractedWith;

        protected bool CanBeMoved { get; set; } = true;

        protected bool IsDragged;
        private Point lastCursorPoint;

        public override void Initialize()
        {
            Color baseColor = UISettings.ActiveSettings.PanelBackgroundColor;
            var backgroundColor = new Color(baseColor.R / 2, baseColor.G / 2, baseColor.B / 2, 222);
            BackgroundTexture = AssetLoader.CreateTexture(backgroundColor, 2, 2);
            PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;

            base.Initialize();
        }

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

            InteractedWith?.Invoke(this, EventArgs.Empty);

            if (UserSettings.Instance.UpscaleUI && Width * 2 <= WindowManager.RenderResolutionX && Height * 2 <= WindowManager.RenderResolutionY)
                Scaling = 2;
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
                    InteractedWith?.Invoke(this, EventArgs.Empty);
                    IsDragged = true;
                    lastCursorPoint = GetCursorPoint();
                }
                else
                {
                    IsDragged = Cursor.LeftDown;

                    if (IsDragged)
                    {
                        Point newCursorPoint = GetCursorPoint();
                        X = X + (newCursorPoint.X - lastCursorPoint.X) * Scaling;
                        Y = Y + (newCursorPoint.Y - lastCursorPoint.Y) * Scaling;
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
                IsDragged = false;
            }
        }
    }
}
