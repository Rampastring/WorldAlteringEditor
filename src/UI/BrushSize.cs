using System;
using TSMapEditor.GameMath;

namespace TSMapEditor.UI
{
    public class BrushSize
    {
        public BrushSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public void DoForBrushSize(Action<Point2D> action)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    action(new Point2D(x, y));
                }
            }
        }
    }
}
