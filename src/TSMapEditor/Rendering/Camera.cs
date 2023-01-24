using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.Input;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering
{
    public class Camera
    {
        private const double ZoomMax = 3.0;
        private const double ZoomMin = 0.2;

        public Camera(WindowManager windowManager, Map map)
        {
            WindowManager = windowManager;
            Map = map;
        }

        public event EventHandler CameraUpdated;

        private readonly WindowManager WindowManager;
        private readonly Map Map;

        private Point2D _topLeftPoint;
        public Point2D TopLeftPoint
        {
            get => _topLeftPoint;
            set
            {
                _topLeftPoint = value;
                _floatTopLeftPoint = new Vector2(_topLeftPoint.X, _topLeftPoint.Y);
                ConstrainCamera();
                CameraUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private Vector2 _floatTopLeftPoint;
        public Vector2 FloatTopLeftPoint 
        {
            get => _floatTopLeftPoint;
            set
            {
                _floatTopLeftPoint = value;
                _topLeftPoint = new Point2D((int)_floatTopLeftPoint.X, (int)_floatTopLeftPoint.Y);
                ConstrainCamera();
                CameraUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                double oldZoom = _zoomLevel;

                if (value > ZoomMax)
                    _zoomLevel = ZoomMax;
                else if (value < ZoomMin)
                    _zoomLevel = ZoomMin;
                else
                    _zoomLevel = value;

                // Adjust camera position so it doesn't change due to the zoom level changing
                if (_zoomLevel != oldZoom)
                {
                    double oldWidth = WindowManager.RenderResolutionX / oldZoom;
                    double newWidth = WindowManager.RenderResolutionX / _zoomLevel;
                    double differenceX = oldWidth - newWidth;

                    double oldHeight = WindowManager.RenderResolutionY / oldZoom;
                    double newHeight = WindowManager.RenderResolutionY / _zoomLevel;
                    double differenceY = oldHeight - newHeight;

                    FloatTopLeftPoint += new Vector2((float)(differenceX / 2.0), (float)(differenceY / 2.0));
                }

                CameraUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ScaleIntWithZoom(int value) => (int)(value * ZoomLevel);

        public void KeyboardUpdate(RKeyboard keyboard, int scrollRate)
        {
            scrollRate = (int)(scrollRate / ZoomLevel);

            if (keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Left))
                TopLeftPoint += new Point2D(-scrollRate, 0);
            else if (keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Right))
                TopLeftPoint += new Point2D(scrollRate, 0);

            if (keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Up))
                TopLeftPoint += new Point2D(0, -scrollRate);
            else if (keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Down))
                TopLeftPoint += new Point2D(0, scrollRate);
        }

        private void ConstrainCamera()
        {
            int minX = (int)((WindowManager.RenderResolutionX / -2) / ZoomLevel);
            if (_topLeftPoint.X < minX)
                _topLeftPoint = new Point2D(minX, _topLeftPoint.Y);

            if (_floatTopLeftPoint.X < minX)
                _floatTopLeftPoint = new Vector2(minX, _floatTopLeftPoint.Y);

            int minY = (int)((WindowManager.RenderResolutionY / -2) / ZoomLevel);
            if (_topLeftPoint.Y < minY)
                _topLeftPoint = new Point2D(_topLeftPoint.X, minY);

            if (_floatTopLeftPoint.Y < minY)
                _floatTopLeftPoint = new Vector2(_floatTopLeftPoint.X, minY);

            int maxX = Map.Size.X * Constants.CellSizeX - (int)((WindowManager.RenderResolutionX / 2) / ZoomLevel);
            if (_topLeftPoint.X > maxX)
                _topLeftPoint = new Point2D(maxX, _topLeftPoint.Y);

            if (_floatTopLeftPoint.X > maxX)
                _floatTopLeftPoint = new Vector2(maxX, _floatTopLeftPoint.Y);

            int maxY = Map.Size.Y * Constants.CellSizeY - (int)((WindowManager.RenderResolutionY / 2) / ZoomLevel);
            if (_topLeftPoint.Y > maxY)
                _topLeftPoint = new Point2D(_topLeftPoint.X, maxY);

            if (_floatTopLeftPoint.Y > maxY)
                _floatTopLeftPoint = new Vector2(_floatTopLeftPoint.X, maxY);
        }
    }
}
