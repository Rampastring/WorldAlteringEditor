using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    /// <summary>
    /// Attaches a <see cref="Tag"/> to a <see cref="MapTile"/>.
    /// </summary>
    public class CellTag
    {
        public Point2D Position { get; set; }
        public Tag Tag { get; set; }
    }
}
