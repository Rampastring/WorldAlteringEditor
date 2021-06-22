using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System.Collections.Generic;
using TSMapEditor.CCEngine;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    class TileDisplayTile
    {
        public TileDisplayTile(Point location, Point offset, Point size, TileImage tileImage)
        {
            Location = location;
            Offset = offset;
            Size = size;
            TileImage = tileImage;
        }

        public Point Location { get; set; }
        public Point Offset { get; set; }
        public Point Size { get; set; }
        public TileImage TileImage { get; set; }
    }

    public class TileDisplay : XNAPanel
    {
        private const int TILE_PADDING = 3;

        public TileDisplay(WindowManager windowManager, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.theaterGraphics = theaterGraphics;
        }

        private readonly TheaterGraphics theaterGraphics;

        private TileSet tileSet;

        private List<TileDisplayTile> tilesInView = new List<TileDisplayTile>();

        int tSetId = 0;

        public override void Initialize()
        {
            base.Initialize();

            BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 196), 2, 2);
            PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
        }

        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.D)
            {
                tSetId++;
                SetTileSet(theaterGraphics.Theater.TileSets[tSetId]);
            }
        }

        public void SetTileSet(TileSet tileSet)
        {
            this.tileSet = tileSet;
            RefreshGraphics();
        }

        private void RefreshGraphics()
        {
            tilesInView.Clear();

            if (tileSet == null)
                return;

            var tilesOnCurrentLine = new List<TileDisplayTile>();
            int usableWidth = Width - (Constants.UIEmptySideSpace * 2);
            int y = Constants.UIEmptyTopSpace;
            int x = Constants.UIEmptySideSpace;
            int currentLineHeight = 0;

            for (int i = 0; i < tileSet.TilesInSet; i++)
            {
                int tileIndex = tileSet.StartTileIndex + i;
                if (tileIndex > theaterGraphics.TileCount)
                    break;

                TileImage tileImage = theaterGraphics.GetTileGraphics(tileIndex);
                if (tileImage == null)
                    break;

                int width = tileImage.GetWidth(out int minX);
                int height = tileImage.GetHeight();

                if (x + width > usableWidth)
                {
                    // Start a new line of tile graphics

                    x = Constants.UIEmptySideSpace;
                    y += currentLineHeight + TILE_PADDING;
                    CenterLine(tilesOnCurrentLine, currentLineHeight);
                    currentLineHeight = 0;
                    tilesOnCurrentLine.Clear();
                }

                if (minX > 0)
                    minX = 0;

                var tileDisplayTile = new TileDisplayTile(new Point(x, y), new Point(-minX, 0), new Point(width, height), tileImage);
                tilesInView.Add(tileDisplayTile);

                if (height > currentLineHeight)
                    currentLineHeight = height;
                x += width + TILE_PADDING;
                tilesOnCurrentLine.Add(tileDisplayTile);
            }

            CenterLine(tilesOnCurrentLine, currentLineHeight);
        }

        /// <summary>
        /// Centers all tiles vertically relative to each other.
        /// </summary>
        private void CenterLine(List<TileDisplayTile> line, int lineHeight)
        {
            foreach (var tile in line)
            {
                tile.Location = new Point(tile.Location.X, tile.Location.Y + (lineHeight - tile.Size.Y) / 2);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            foreach (var tile in tilesInView)
            {
                FillRectangle(new Rectangle(tile.Location.X, tile.Location.Y, tile.Size.X, tile.Size.Y), Color.Black);

                foreach (MGTMPImage image in tile.TileImage.TMPImages)
                {
                    if (image == null || image.TmpImage == null)
                        continue;

                    DrawTexture(image.Texture, new Rectangle(tile.Location.X + image.TmpImage.X + tile.Offset.X,
                        tile.Location.Y + image.TmpImage.Y + tile.Offset.Y,
                        Constants.CellSizeX, Constants.CellSizeY), Color.White);
                }
            }
        }
    }
}
