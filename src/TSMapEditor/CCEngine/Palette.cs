namespace TSMapEditor.CCEngine
{
    public class Palette
    {
        private const int LENGTH = 256;

        public Palette(byte[] buffer)
        {
            Data = new RGBColor[LENGTH];
            Parse(buffer);
        }

        public RGBColor[] Data;

        public void Parse(byte[] buffer)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = new RGBColor(buffer, i * 3, 2);
            }
        }
    }
}
