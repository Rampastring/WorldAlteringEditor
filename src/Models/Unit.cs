namespace TSMapEditor.Models
{
    public class Unit : Foot
    {
        // [Units]
        // INDEX=OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,HIGH,FOLLOWS_INDEX,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

        public Unit(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public int FollowsID { get; set; }
        public Unit FollowedUnit { get; set; }
    }
}
