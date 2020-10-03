using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.CCEngine
{
    /// <summary>
    /// A Tiberian Sun TMP file.
    /// </summary>
    public class TmpFile
    {
        public TmpFile() { }

        private TmpFileHeader tmpFileHeader;
        private List<TmpImage> tmpImages = new List<TmpImage>();

        public int ImageCount => tmpImages.Count;
        public TmpImage GetImage(int id) => tmpImages[id];

        public void ParseFromFile(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                Parse(stream);
            }
        }

        public void Parse(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buffer, 0, buffer.Length);
            ParseFromBuffer(buffer);
        }

        public void ParseFromBuffer(byte[] buffer)
        {
            tmpFileHeader = new TmpFileHeader(buffer);
            int tileCount = tmpFileHeader.Width * tmpFileHeader.Height;

            List<int> tmpHeaderOffsets = new List<int>();

            for (int i = 0; i < tileCount; i++)
            {
                int offset = BitConverter.ToInt32(buffer, TmpFileHeader.SIZE + i * 4);
                tmpHeaderOffsets.Add(offset);
            }

            MemoryStream memoryStream = new MemoryStream(buffer);

            for (int i = 0; i < tileCount; i++)
            {
                if (tmpHeaderOffsets[i] == 0)
                {
                    tmpImages.Add(null);
                }
                else
                {
                    memoryStream.Position = tmpHeaderOffsets[i];
                    TmpImage tmpImage = new TmpImage(memoryStream);
                    tmpImages.Add(tmpImage);
                }
            }
        }
    }

    /// <summary>
    /// A TMP file header.
    /// </summary>
    struct TmpFileHeader
    {
        public const int SIZE = 16;

        public TmpFileHeader(byte[] buffer)
        {
            if (buffer.Length < SIZE)
                throw new ArgumentException("buffer is not long enough");

            Width = BitConverter.ToInt32(buffer, 0);
            Height = BitConverter.ToInt32(buffer, 4);
            TileWidth = BitConverter.ToInt32(buffer, 8);
            TileHeight = BitConverter.ToInt32(buffer, 12);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
    }

    /// <summary>
    /// A single TMP image.
    /// </summary>
    public class TmpImage
    {
        public const int IMAGE_HEADER_SIZE = 48;
        public const int COLOR_SIZE = 576;

        public TmpImage(Stream stream)
        {
            if (stream.Length < stream.Position + IMAGE_HEADER_SIZE + COLOR_SIZE)
                throw new ArgumentException("buffer is not long enough");

            X = ReadIntFromStream(stream);
            Y = ReadIntFromStream(stream);
            ExtraDataOffset = ReadUIntFromStream(stream);
            ZDataOffset = ReadUIntFromStream(stream);
            ExtraZDataOffset = ReadUIntFromStream(stream);
            XExtra = ReadIntFromStream(stream);
            YExtra = ReadIntFromStream(stream);
            ExtraWidth = ReadUIntFromStream(stream);
            ExtraHeight = ReadUIntFromStream(stream);
            stream.Read(buffer, 0, 4);
            ImageFlags = (TmpImageFlags)BitConverter.ToUInt32(buffer, 0);
            stream.Read(buffer, 0, 3);
            Height = buffer[0];
            TerrainType = buffer[1];
            RampType = buffer[2];
            stream.Read(buffer, 0, 3);
            RadarLeftColor = new RGBColor(buffer, 0);
            stream.Read(buffer, 0, 3);
            RadarRightColor = new RGBColor(buffer, 0);
            stream.Read(buffer, 0, Unknown.Length);
            Array.ConstrainedCopy(buffer, 0, Unknown, 0, Unknown.Length);
            stream.Read(ColorData, 0, COLOR_SIZE);

            //ExtraGraphicsData = new byte[ExtraWidth * ExtraHeight];
            //stream.Read(ExtraGraphicsData, 0, ExtraGraphicsData.Length);
        }

        private int ReadIntFromStream(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        private uint ReadUIntFromStream(Stream stream)
        {
            stream.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        byte[] buffer = new byte[4];

        public int X { get; private set; }
        public int Y { get; private set; }
        public uint ExtraDataOffset { get; private set; }
        public uint ZDataOffset { get; private set; }
        public uint ExtraZDataOffset { get; private set; }
        public int XExtra { get; private set; }
        public int YExtra { get; private set; }
        public uint ExtraWidth { get; private set; }
        public uint ExtraHeight { get; private set; }
        public TmpImageFlags ImageFlags { get; private set; }
        public byte Height { get; private set; }
        public byte TerrainType { get; private set; }
        public byte RampType { get; private set; }
        public RGBColor RadarLeftColor { get; set; }
        public RGBColor RadarRightColor { get; set; }
        public byte[] Unknown = new byte[3];

        public byte[] ColorData = new byte[COLOR_SIZE];
        public byte[] ExtraGraphicsData = new byte[0];
    }

    [Flags]
    public enum TmpImageFlags
    {
        NONE = 0,
        IS_RANDOMIZED = 2,
        HAS_Z_DATA = 4,
        HAS_EXTRA_DATA = 8
    }
}
