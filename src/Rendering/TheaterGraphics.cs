using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using TSMapEditor.CCEngine;

namespace TSMapEditor.Rendering
{
    public class TileImage
    {
        public TileImage(int tileSetId, int tileIndex, MGTMPImage[] tMPImages)
        {
            TileSetId = tileSetId;
            TileIndex = tileIndex;
            TMPImages = tMPImages;
        }

        public int TileSetId { get; set; }
        public int TileIndex { get; set; }
        public MGTMPImage[] TMPImages { get; set; }
    }

    /// <summary>
    /// A wrapper for a theater that loads and stores tile graphics.
    /// </summary>
    public class TheaterGraphics
    {
        public TheaterGraphics(GraphicsDevice graphicsDevice, Theater theater, CCFileManager fileManager)
        {
            Theater = theater;
            byte[] paletteData = fileManager.LoadFile(theater.PaletteName);
            palette = new Palette(paletteData);

            for (int tsId = 0; tsId < theater.TileSets.Count; tsId++)
            {
                TileSet tileSet = theater.TileSets[tsId];

                Console.WriteLine("Loading " + tileSet.SetName);

                for (int i = 0; i < tileSet.TilesInSet; i++)
                {
                    Console.WriteLine("#" + i);

                    // Handle graphics variation (clear00.tem, clear00a.tem, clear00b.tem etc.)
                    for (int v = 0; v < 'g' - 'a'; v++)
                    {
                        string baseName = tileSet.FileName + (i + 1).ToString("D2", CultureInfo.InvariantCulture);

                        if (v > 0)
                        {
                            baseName = baseName + ((char)('a' + (v - 1)));
                        }

                        byte[] data = fileManager.LoadFile(baseName + theater.FileExtension);

                        if (data == null)
                        {
                            if (v == 0)
                            {
                                graphicsList.Add(new TileImage(tsId, i, new MGTMPImage[0]));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }

                        var tmpFile = new TmpFile();
                        tmpFile.ParseFromBuffer(data);

                        var tmpImages = new List<MGTMPImage>();
                        for (int img = 0; img < tmpFile.ImageCount; img++)
                        {
                            tmpImages.Add(new MGTMPImage(graphicsDevice, tmpFile.GetImage(img), palette, tsId));
                        }
                        graphicsList.Add(new TileImage(tsId, i, tmpImages.ToArray()));
                        break;
                    }
                }
            }
        }

        public Theater Theater { get; }

        private Palette palette;

        private List<TileImage> graphicsList = new List<TileImage>();

        public int TileCount => graphicsList.Count;

        public TileImage GetTileGraphics(int id) => graphicsList[id];
    }
}