namespace TSMapEditor.Models
{
    public class Infantry : Foot
    {
        // [Infantry]
        // INDEX=OWNER,ID,HEALTH,X,Y,SUB_CELL,MISSION,FACING,TAG,VETERANCY,GROUP,HIGH,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

        public Infantry(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Infantry;

        public SubCell SubCell { get; set; }
    }
}
