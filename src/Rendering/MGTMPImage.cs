using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TSMapEditor.CCEngine;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// A MonoGame-drawable TMP image.
    /// Contains graphics for a single cell (sub-tile of a full TMP).
    /// </summary>
    public class MGTMPImage
    {
        public MGTMPImage(GraphicsDevice gd, TmpImage tmpImage, Palette palette, int tileSetId)
        {
            if (tmpImage != null)
            {
                TmpImage = tmpImage;
                Palette = palette;
                Texture = TextureFromTmpImage(gd, tmpImage, palette);
            }

            TileSetId = tileSetId;
        }

        public Texture2D Texture { get; }

        public int TileSetId { get; }
        public TmpImage TmpImage { get; private set; }
        public Palette Palette { get; private set; }

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
                    colorData[i * Constants.CellSizeX + xPos] = XNAColorFromRGBColor(palette.Data[image.ColorData[tmpPixelIndex]]);
                    xPos++;
                    tmpPixelIndex++;
                }

                if (i < 11)
                    w += 4;
                else
                    w -= 4;
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
