using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A base class for game objects.
    /// Represents ObjectClass in the original game's class hierarchy.
    /// </summary>
    public abstract class GameObject : AbstractObject
    {
        public Point2D Position { get; set; }
    }
}
