namespace TSMapEditor.Models
{
    /// <summary>
    /// A base class for game objects.
    /// Represents ObjectClass in the original game's class hierarchy.
    /// </summary>
    public abstract class GameObject : AbstractObject
    {
        public GameObject(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }
    }
}
