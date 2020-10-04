using Microsoft.Xna.Framework;

namespace TSMapEditor.GameMath
{
    public struct Point2D
    {
        public int X;
        public int Y;

        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point2D operator +(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point2D FromXNAPoint(Point point)
            => new Point2D(point.X, point.Y);
    }
}
