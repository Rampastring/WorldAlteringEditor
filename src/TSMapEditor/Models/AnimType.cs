namespace TSMapEditor.Models
{
    public class AnimType : GameObjectType
    {
        public AnimType(string iniName) : base(iniName) { }

        public override RTTIType WhatAmI() => RTTIType.AnimType;
    }
}
