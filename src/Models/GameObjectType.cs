namespace TSMapEditor.Models
{
    public abstract class GameObjectType : AbstractObject, INIDefined
    {
        public GameObjectType(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }
        public int Index { get; set; }
    }
}
