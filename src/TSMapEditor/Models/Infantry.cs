namespace TSMapEditor.Models
{
    public class Infantry : Foot<InfantryType>
    {
        public Infantry(InfantryType objectType) : base(objectType)
        {
        }

        // [Infantry]
        // INDEX=OWNER,ID,HEALTH,X,Y,SUB_CELL,MISSION,FACING,TAG,VETERANCY,GROUP,HIGH,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

        public override RTTIType WhatAmI() => RTTIType.Infantry;

        public SubCell SubCell { get; set; }

        public override int GetFrameIndex(int frameCount)
        {
            if (ObjectType.ArtConfig.Sequence == null)
                return 0;

            var readySequence = ObjectType.ArtConfig.Sequence.Ready;
            int frame = readySequence.StartFrame + ((Facing / 32) * readySequence.FacingMultiplier * readySequence.FrameCount);

            return frame;
        }

        public override int GetShadowFrameIndex(int frameCount)
        {
            return GetFrameIndex(frameCount) + (frameCount / 2);
        }
    }
}
