namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'vehicle', but let's respect the original game's naming.
    /// </summary>
    public class Unit : Foot<UnitType>
    {
        public Unit(UnitType objectType) : base(objectType)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public UnitType UnitType { get; private set; }
        public int FollowsID { get; set; }
        public Unit FollowedUnit { get; set; }
    }
}
