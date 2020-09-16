namespace TSMapEditor.Models
{
    public class House : AbstractObject
    {
        public override RTTIType WhatAmI() => RTTIType.HouseType;

        public string ININame { get; set; }
        public int IQ { get; set; }
        public string Color { get; set; }
        public string Allies { get; set; }
        public int Credits { get; set; }
        public int ActsLike { get; set; }
        public int TechLevel { get; set; }
        public int PercentBuilt { get; set; }
        public bool PlayerControl { get; set; }
    }
}
