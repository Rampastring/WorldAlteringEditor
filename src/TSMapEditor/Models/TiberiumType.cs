using Microsoft.Xna.Framework;

namespace TSMapEditor.Models
{
    public class TiberiumType : INIDefineable
    {
        public TiberiumType(string iniName, int index)
        {
            ININame = iniName;
            Index = index;
        }

        public string ININame { get; }
        public int Index { get; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int Power { get; set; }
        public int Image { get; set; }
        public int Growth { get; set; }
        public double GrowthPercentage { get; set; }
        public int Spread { get; set; }
        public int SpreadPercentage { get; set; }
        public string Color { get; set; } = string.Empty;
        public Color XNAColor { get; set; }
    }
}
