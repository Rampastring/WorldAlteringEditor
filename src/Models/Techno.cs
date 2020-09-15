using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    public abstract class Techno : GameObject
    {
        public Techno(string iniName) : base(iniName)
        {
        }

        public House Owner { get; set; }
        public byte HP { get; set; }
        public Point2D Position { get; set; }
        public byte Facing { get; set; }
        public string Tag { get; set; }
    }
}
