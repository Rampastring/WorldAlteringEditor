using Microsoft.Xna.Framework;
using System;

namespace TSMapEditor.Models
{
    public abstract class Techno<T> : TechnoBase where T : TechnoType
    {
        public Techno(T objectType)
        {
            ObjectType = objectType;
        }

        public override double GetWeaponRange() => ObjectType.GetWeaponRange();

        public T ObjectType { get; }
    }

    public abstract class TechnoBase : GameObject
    {
        public TechnoBase()
        {
            HP = Constants.ObjectHealthMax;
        }

        public House Owner { get; set; }
        public int HP { get; set; }
        public byte Facing { get; set; }
        public Tag AttachedTag { get; set; }

        public abstract double GetWeaponRange();

        public override Color GetRemapColor() => Remapable() ? Owner.XNAColor : Color.White;
    }
}
