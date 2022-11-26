using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSMapEditor.CCEngine;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// Interface for a single cell of a tile; sub-tile of a full TMP.
    /// </summary>
    public interface ISubTileImage
    {
        TmpImage TmpImage { get; }
        Palette Palette { get; }
    }

    /// <summary>
    /// A MonoGame-drawable TMP image.
    /// Contains graphics and information for a single cell (sub-tile of a full TMP).
    /// </summary>
    public class MGTMPImage : ISubTileImage
    {
        public MGTMPImage(GraphicsDevice gd, TmpImage tmpImage, Palette palette, int tileSetId)
        {
            if (tmpImage != null)
            {
                TmpImage = tmpImage;
                Palette = palette;
                Texture = TextureFromTmpImage(gd, tmpImage, palette);

                if (tmpImage.ExtraGraphicsColorData != null && tmpImage.ExtraGraphicsColorData.Length > 0)
                {
                    ExtraTexture = TextureFromExtraTmpData(gd, tmpImage, palette);
                }
            }

            TileSetId = tileSetId;
        }

        public Texture2D Texture { get; }
        public Texture2D ExtraTexture { get; }

        public int TileSetId { get; }
        public TmpImage TmpImage { get; private set; }
        public Palette Palette { get; private set; }


        public void Dispose()
        {
            if (Texture != null)
                Texture.Dispose();
        }

        private Texture2D TextureFromTmpImage(GraphicsDevice graphicsDevice, TmpImage image, Palette palette)
        {
            Texture2D texture = new Texture2D(graphicsDevice, Constants.CellSizeX, Constants.CellSizeY, false, SurfaceFormat.Color);
            Color[] colorData = new Color[Constants.CellSizeX * Constants.CellSizeY];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Color.Transparent;
            }

            int tmpPixelIndex = 0;
            int w = 4;
            for (int i = 0; i < Constants.CellSizeY; i++)
            {
                int xPos = Constants.CellSizeY - (w / 2);
                for (int x = 0; x < w; x++)
                {
                    if (image.ColorData[tmpPixelIndex] > 0)
                    {
                        colorData[i * Constants.CellSizeX + xPos] = XNAColorFromRGBColor(palette.Data[image.ColorData[tmpPixelIndex]]);
                    }
                    
                    xPos++;
                    tmpPixelIndex++;
                }

                if (i < (Constants.CellSizeY / 2) - 1)
                    w += 4;
                else
                    w -= 4;
            }

            texture.SetData(colorData);
            return texture;
        }

        private Texture2D TextureFromExtraTmpData(GraphicsDevice graphicsDevice, TmpImage image, Palette palette)
        {
            int width = (int)image.ExtraWidth;
            int height = (int)image.ExtraHeight;

            var texture = new Texture2D(graphicsDevice, width, height);
            Color[] colorData = new Color[width * height];
            for (int i = 0; i < colorData.Length; i++)
            {
                colorData[i] = Color.Transparent;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;

                    if (image.ExtraGraphicsColorData[index] > 0)
                    {
                        colorData[index] = XNAColorFromRGBColor(palette.Data[image.ExtraGraphicsColorData[index]]);
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }

        private Color XNAColorFromRGBColor(RGBColor color)
        {
            return new Color(color.R, color.G, color.B, (byte)255);
        }
    }
}
