namespace TSMapEditor.Rendering.ObjectRenderers
{
    public struct CommonDrawParams
    {
        public ObjectImage Graphics;
        public string IniName;

        public CommonDrawParams(ObjectImage graphics, string iniName)
        {
            Graphics = graphics;
            IniName = iniName;
        }
    }
}
