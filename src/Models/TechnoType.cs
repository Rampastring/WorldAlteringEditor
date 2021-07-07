namespace TSMapEditor.Models
{
    public abstract class TechnoType : GameObjectType
    {
        public TechnoType(string iniName) : base(iniName)
        {
        }

        public string Image { get; set; }
        public string Owner { get; set; }
    }
}
