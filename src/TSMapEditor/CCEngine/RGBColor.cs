using Microsoft.Xna.Framework;

namespace TSMapEditor.CCEngine
{
    public struct RGBColor
    {
        public RGBColor(byte[] buffer, int offset)
        {
            R = (byte)(buffer[offset] << 2);
            G = (byte)(buffer[offset + 1] << 2);
            B = (byte)(buffer[offset + 2] << 2);
        }

        public RGBColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R;
        public byte G;
        public byte B;

        public Color ToXnaColor() => new Color(R, G, B);
    }
}
