using Microsoft.Xna.Framework;

namespace TSMapEditor.Models
{
    public class House : AbstractObject
    {
        public override RTTIType WhatAmI() => RTTIType.HouseType;

        public House(string iniName)
        {
            ININame = iniName;
        }

        [INI(false)]
        public string ININame { get; set; }
        public int IQ { get; set; }
        public string Edge { get; set; }
        public string Color { get; set; } = "White";
        public string Allies { get; set; }
        public int Credits { get; set; }
        public int ActsLike { get; set; } = -1;
        public int TechLevel { get; set; }
        public int PercentBuilt { get; set; }
        public bool PlayerControl { get; set; }
        public string Side { get; set; }
        public int ID { get; set; }

        [INI(false)]
        public Color XNAColor { get; set; } = Microsoft.Xna.Framework.Color.White;
    }
}
