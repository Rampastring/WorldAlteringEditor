namespace TSMapEditor.Rendering.ObjectRenderers
{
    public interface ICommonDrawParams
    {
        public string IniName { get; }
    }

    public struct VoxelDrawParams : ICommonDrawParams
    {
        public string IniName { get; }
        public VoxelModel Graphics;

        public VoxelDrawParams(VoxelModel graphics, string iniName)
        {
            Graphics = graphics;
            IniName = iniName;
        }
    }

    public struct ShapeDrawParams : ICommonDrawParams
    {
        public string IniName { get; }
        public ShapeImage Graphics;

        public ShapeDrawParams(ShapeImage graphics, string iniName)
        {
            Graphics = graphics;
            IniName = iniName;
        }
    }
}
