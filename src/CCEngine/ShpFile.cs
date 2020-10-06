using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.CCEngine
{
    [Flags]
    public enum ShpCompression
    {
        HasTransparency = 1,
        UsesRle = 2
    }

    struct ShpFileHeader
    {
        private const int SizeOf = 8;

        public ShpFileHeader(byte[] buffer)
        {
            if (buffer.Length < SizeOf)
                throw new ArgumentException(nameof(ShpFileHeader) + ": buffer is not long enough");

            Unknown = BitConverter.ToUInt16(buffer, 0);
            if (Unknown != 0)
                throw new ArgumentException("Unexpected field value in SHP header");

            SpriteSheetWidth = BitConverter.ToUInt16(buffer, 2);
            SpriteSheetHeight = BitConverter.ToUInt16(buffer, 4);
            FrameCount = BitConverter.ToUInt16(buffer, 6);
        }

        public ushort Unknown;
        public ushort SpriteSheetWidth;
        public ushort SpriteSheetHeight;
        public ushort FrameCount;
    }

    class ShpFrameInfo
    {
        private const int SizeOf = 24;

        public ShpFrameInfo(byte[] buffer)
        {
            if (buffer.Length < SizeOf)
                throw new ArgumentException(nameof(ShpFrameInfo) + ": buffer is not long enough");

            XOffset = BitConverter.ToUInt16(buffer, 0);
            YOffset = BitConverter.ToUInt16(buffer, 2);
            Width = BitConverter.ToUInt16(buffer, 4);
            Height = BitConverter.ToUInt16(buffer, 6);
            Flags = (ShpCompression)BitConverter.ToUInt32(buffer, 8);
            AverageColor = new RGBColor(buffer[12], buffer[13], buffer[14]);
            Unknown1 = buffer[15];
            Unknown2 = BitConverter.ToUInt32(buffer, 16);
            DataOffset = BitConverter.ToUInt32(buffer, 20);
        }

        public ushort XOffset;
        public ushort YOffset;
        public ushort Width;
        public ushort Height;
        public ShpCompression Flags;
        public RGBColor AverageColor;
        public byte Unknown1;
        public uint Unknown2;
        public uint DataOffset;
    }

    public class ShpFile
    {

    }


}
