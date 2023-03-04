using Microsoft.Xna.Framework;

namespace TSMapEditor.Models
{
    public abstract class Techno<T> : TechnoBase where T : GameObjectType
    {
        public Techno(T objectType)
        {
            ObjectType = objectType;
        }

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

        public override Color GetRemapColor() => Remapable() ? Owner.XNAColor : Color.White;
    }
}
