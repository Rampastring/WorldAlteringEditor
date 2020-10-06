using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
