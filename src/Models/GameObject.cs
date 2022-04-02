using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    public interface IMovable : IPositioned
    {
        RTTIType WhatAmI();

        bool IsTechno();
    }

    /// <summary>
    /// A base class for game objects.
    /// Represents ObjectClass in the original game's class hierarchy.
    /// </summary>
    public abstract class GameObject : AbstractObject, IMovable
    {
        public Point2D Position { get; set; }

        public virtual int GetYDrawOffset()
        {
            return 0;
        }

        public virtual int GetFrameIndex(int frameCount)
        {
            return 0;
        }

        public virtual int GetShadowFrameIndex(int frameCount)
        {
            return (frameCount / 2);
        }

        public virtual int GetXPositionForDrawOrder()
        {
            return Position.X;
        }

        public virtual int GetYPositionForDrawOrder()
        {
            return Position.Y;
        }
    }
}
