namespace TSMapEditor.Models
{
    public class Unit : Foot<UnitType>
    {
        public Unit(UnitType objectType) : base(objectType)
        {
        }

        // [Units]
        // INDEX=OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,HIGH,FOLLOWS_INDEX,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public UnitType UnitType { get; private set; }
        public int FollowsID { get; set; }
        public Unit FollowedUnit { get; set; }
    }
}
