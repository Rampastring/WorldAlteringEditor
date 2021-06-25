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
        public House Owner { get; set; }
        public int HP { get; set; }
        public byte Facing { get; set; }
        public Tag AttachedTag { get; set; }
    }
}
