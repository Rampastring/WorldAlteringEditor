using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// Contains graphics for a single full TMP (all sub-tiles / all cells).
    /// </summary>
    public class TileImage
    {
        public TileImage(int tileSetId, int tileIndex, MGTMPImage[] tmpImages)
        {
            TileSetId = tileSetId;
            TileIndex = tileIndex;
            TMPImages = tmpImages;
        }

        public int TileSetId { get; set; }
        public int TileIndex { get; set; }
        public MGTMPImage[] TMPImages { get; set; }

        /// <summary>
        /// Calculates and returns the width of this full tile image.
        /// </summary>
        public int GetWidth(out int outMinX)
        {
            outMinX = 0;

            if (TMPImages == null)
                return 0;

            int maxX = int.MinValue;
            int minX = int.MaxValue;
            for (int i = 0; i < TMPImages.Length; i++)
            {
                if (TMPImages[i] == null)
                    continue;

                var tmpData = TMPImages[i].TmpImage;
                if (tmpData == null)
                    continue;

                if (tmpData.X < minX)
                    minX = tmpData.X;

                int cellRightXCoordinate = tmpData.X + Constants.CellSizeX;
                if (cellRightXCoordinate > maxX)
                    maxX = cellRightXCoordinate;
            }

            outMinX = minX;
            return Math.Abs(minX) + maxX;
        }

        /// <summary>
        /// Calculates and returns the height of this full tile image.
        /// </summary>
        public int GetHeight()
        {
            if (TMPImages == null)
                return 0;

            int height = 0;
            for (int i = 0; i < TMPImages.Length; i++)
            {
                if (TMPImages[i] == null)
                    continue;

                var tmpData = TMPImages[i].TmpImage;
                if (tmpData == null)
                    continue;

                int cellBottomCoordinate = tmpData.Y + Constants.CellSizeY;
                if (cellBottomCoordinate > height)
                    height = cellBottomCoordinate;
            }

            return height;
        }
    }

    public class ObjectImage
    {
        public ObjectImage(GraphicsDevice graphicsDevice, ShpFile shp, byte[] shpFileData, Palette palette, List<int> framesToLoad = null)
        {
            Frames = new PositionedTexture[shp.FrameCount];
            for (int i = 0; i < shp.FrameCount; i++)
            {
                if (framesToLoad != null && !framesToLoad.Contains(i))
                    continue;

                var frameInfo = shp.GetShpFrameInfo(i);
                var frameData = shp.GetUncompressedFrameData(i, shpFileData);
                if (frameData == null)
                    continue;

                var texture = new Texture2D(graphicsDevice, frameInfo.Width, frameInfo.Height, false, SurfaceFormat.Color);
                Color[] colorArray = frameData.Select(b => b == 0 ? Color.Transparent : palette.Data[b].ToXnaColor()).ToArray();
                texture.SetData<Color>(colorArray);
                Frames[i] = new PositionedTexture(shp.Width, shp.Height, frameInfo.XOffset, frameInfo.YOffset, texture);
            }
        }

        public PositionedTexture[] Frames { get; set; }
    }

    public class PositionedTexture
    {
        public int ShapeWidth;
        public int ShapeHeight;
        public int OffsetX;
        public int OffsetY;
        public Texture2D Texture;

        public PositionedTexture(int shapeWidth, int shapeHeight, int offsetX, int offsetY, Texture2D texture)
        {
            ShapeWidth = shapeWidth;
            ShapeHeight = shapeHeight;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Texture = texture;
        }
    }

    /// <summary>
    /// A wrapper for a theater that loads and stores tile and object graphics.
    /// </summary>
    public class TheaterGraphics
    {
        private const string SHP_FILE_EXTENSION = ".SHP";

        public TheaterGraphics(GraphicsDevice graphicsDevice, Theater theater, CCFileManager fileManager, Rules rules)
        {
            Theater = theater;
            this.fileManager = fileManager;

            theaterPalette = GetPaletteOrFail(theater.PaletteName);
            unitPalette = GetPaletteOrFail(Theater.UnitPaletteName);

            var task1 = Task.Factory.StartNew(() => ReadTileTextures(graphicsDevice));
            var task2 = Task.Factory.StartNew(() => ReadTerrainObjectTextures(graphicsDevice, rules.TerrainTypes));
            var task3 = Task.Factory.StartNew(() => ReadBuildingTextures(graphicsDevice, rules.BuildingTypes));
            var task4 = Task.Factory.StartNew(() => ReadUnitTextures(graphicsDevice, rules.UnitTypes));
            var task5 = Task.Factory.StartNew(() => ReadInfantryTextures(graphicsDevice, rules.InfantryTypes));
            var task6 = Task.Factory.StartNew(() => ReadOverlayTextures(graphicsDevice, rules.OverlayTypes));
            Task.WaitAll(task1, task2, task3, task4, task5, task6);
        }

        private void ReadTileTextures(GraphicsDevice graphicsDevice)
        {
            int currentTileIndex = 0; // Used for setting the starting tile ID of a tileset

            for (int tsId = 0; tsId < Theater.TileSets.Count; tsId++)
            {
                TileSet tileSet = Theater.TileSets[tsId];
                tileSet.StartTileIndex = currentTileIndex;
                tileSet.LoadedTileCount = 0;

                Console.WriteLine("Loading " + tileSet.SetName);

                for (int i = 0; i < tileSet.TilesInSet; i++)
                {
                    Console.WriteLine("#" + i);

                    var tileGraphics = new List<TileImage>();

                    // Handle graphics variation (clear00.tem, clear00a.tem, clear00b.tem etc.)
                    for (int v = 0; v < 'g' - 'a'; v++)
                    {
                        string baseName = tileSet.FileName + (i + 1).ToString("D2", CultureInfo.InvariantCulture);

                        if (v > 0)
                        {
                            baseName = baseName + ((char)('a' + (v - 1)));
                        }

                        byte[] data = fileManager.LoadFile(baseName + Theater.FileExtension);

                        if (data == null)
                        {
                            if (v == 0)
                            {
                                tileGraphics.Add(new TileImage(tsId, i, new MGTMPImage[0]));
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
                            tmpImages.Add(new MGTMPImage(graphicsDevice, tmpFile.GetImage(img), theaterPalette, tsId));
                        }
                        tileGraphics.Add(new TileImage(tsId, i, tmpImages.ToArray()));
                    }

                    tileSet.LoadedTileCount++;
                    graphicsList.Add(tileGraphics.ToArray());
                }

                currentTileIndex += tileSet.TilesInSet;
            }
        }

        public void ReadTerrainObjectTextures(GraphicsDevice graphicsDevice, List<TerrainType> terrainTypes)
        {
            var unitPalette = GetPaletteOrFail(Theater.UnitPaletteName);

            TerrainObjectTextures = new ObjectImage[terrainTypes.Count];
            for (int i = 0; i < terrainTypes.Count; i++)
            {
                string shpFileName = terrainTypes[i].Image != null ? terrainTypes[i].Image : terrainTypes[i].ININame;
                if (terrainTypes[i].Theater)
                    shpFileName += Theater.FileExtension;
                else
                    shpFileName += SHP_FILE_EXTENSION;

                byte[] data = fileManager.LoadFile(shpFileName);
                if (data == null)
                    continue;

                var shpFile = new ShpFile();
                shpFile.ParseFromBuffer(data);
                TerrainObjectTextures[i] = new ObjectImage(graphicsDevice, shpFile, data, 
                    terrainTypes[i].SpawnsTiberium ? unitPalette : theaterPalette);
            }
        }

        public void ReadBuildingTextures(GraphicsDevice graphicsDevice, List<BuildingType> buildingTypes)
        {
            BuildingTextures = new ObjectImage[buildingTypes.Count];
            for (int i = 0; i < buildingTypes.Count; i++)
            {
                var buildingType = buildingTypes[i];

                string shpFileName = string.IsNullOrWhiteSpace(buildingType.Image) ? buildingType.ININame : buildingType.Image;
                if (buildingType.ArtConfig.Theater)
                    shpFileName += Theater.FileExtension;
                else
                    shpFileName += SHP_FILE_EXTENSION;

                byte[] shpData = null;
                if (buildingType.ArtConfig.NewTheater)
                {
                    string newTheaterShpName = shpFileName.Substring(0, 1) + Theater.NewTheaterBuildingLetter + shpFileName.Substring(2);

                    shpData = fileManager.LoadFile(newTheaterShpName);
                }

                // The game can apparently fall back to the non-theater-specific SHP file name
                // if the theater-specific SHP is not found
                if (shpData == null)
                {
                    shpData = fileManager.LoadFile(shpFileName);
                    if (shpData == null)
                    {
                        continue;
                    }
                }

                var shpFile = new ShpFile();
                shpFile.ParseFromBuffer(shpData);
                BuildingTextures[i] = new ObjectImage(graphicsDevice, shpFile, shpData,
                    buildingType.ArtConfig.TerrainPalette ? theaterPalette : unitPalette);
            }
        }

        public void ReadUnitTextures(GraphicsDevice graphicsDevice, List<UnitType> unitTypes)
        {
            var loadedTextures = new Dictionary<string, ObjectImage>();
            UnitTextures = new ObjectImage[unitTypes.Count];

            for (int i = 0; i < unitTypes.Count; i++)
            {
                var unitType = unitTypes[i];

                string shpFileName = string.IsNullOrWhiteSpace(unitType.Image) ? unitType.ININame : unitType.Image;
                shpFileName += SHP_FILE_EXTENSION;
                if (loadedTextures.TryGetValue(shpFileName, out ObjectImage loadedImage))
                {
                    UnitTextures[i] = loadedImage;
                    continue;
                }

                byte[] shpData = fileManager.LoadFile(shpFileName);

                if (shpData == null)
                    continue;

                // We don't need firing frames and some other stuff,
                // so we build a list of frames to load to save VRAM
                var framesToLoad = unitType.GetIdleFrameIndexes();
                if (unitType.Turret)
                {
                    int turretStartFrame = unitType.GetTurretStartFrame();
                    const int TURRET_FRAME_COUNT = 32;
                    for (int t = turretStartFrame; t < turretStartFrame + TURRET_FRAME_COUNT; t++)
                        framesToLoad.Add(t);
                }

                var shpFile = new ShpFile();
                shpFile.ParseFromBuffer(shpData);

                // Load shadow frames
                int regularFrameCount = framesToLoad.Count;
                for (int j = 0; j < regularFrameCount; j++)
                    framesToLoad.Add(framesToLoad[j] + (shpFile.FrameCount / 2));

                UnitTextures[i] = new ObjectImage(graphicsDevice, shpFile, shpData, unitPalette, framesToLoad);
                loadedTextures[shpFileName] = UnitTextures[i];
            }
        }

        public void ReadInfantryTextures(GraphicsDevice graphicsDevice, List<InfantryType> infantryTypes)
        {
            var loadedTextures = new Dictionary<string, ObjectImage>();
            InfantryTextures = new ObjectImage[infantryTypes.Count];

            for (int i = 0; i < infantryTypes.Count; i++)
            {
                var infantryType = infantryTypes[i];

                string image = string.IsNullOrWhiteSpace(infantryType.Image) ? infantryType.ININame : infantryType.Image;
                string shpFileName = string.IsNullOrWhiteSpace(infantryType.ArtConfig.Image) ? image : infantryType.ArtConfig.Image;
                shpFileName += SHP_FILE_EXTENSION;
                if (loadedTextures.TryGetValue(shpFileName, out ObjectImage loadedImage))
                {
                    InfantryTextures[i] = loadedImage;
                    continue;
                }

                if (infantryType.ArtConfig.Sequence == null)
                {
                    continue;
                }

                byte[] shpData = fileManager.LoadFile(shpFileName);

                if (shpData == null)
                    continue;

                var framesToLoad = new List<int>();
                const int FACING_COUNT = 8;
                var readySequence = infantryType.ArtConfig.Sequence.Ready;
                for (int j = 0; j < FACING_COUNT; j++)
                {
                    framesToLoad.Add(readySequence.StartFrame + (readySequence.FrameCount * readySequence.FacingMultiplier * j));
                }

                var shpFile = new ShpFile();
                shpFile.ParseFromBuffer(shpData);

                // Load shadow frames
                int regularFrameCount = framesToLoad.Count;
                for (int j = 0; j < regularFrameCount; j++)
                    framesToLoad.Add(framesToLoad[j] + (shpFile.FrameCount / 2));

                InfantryTextures[i] = new ObjectImage(graphicsDevice, shpFile, shpData, unitPalette, null);
                loadedTextures[shpFileName] = InfantryTextures[i];
            }
        }

        public void ReadOverlayTextures(GraphicsDevice graphicsDevice, List<OverlayType> overlayTypes)
        {
            OverlayTextures = new ObjectImage[overlayTypes.Count];
            for (int i = 0; i < overlayTypes.Count; i++)
            {
                var overlayType = overlayTypes[i];

                string imageName = string.IsNullOrWhiteSpace(overlayType.Image) ? overlayType.ININame : overlayType.Image;
                string fileExtension = overlayType.ArtConfig.Theater ? Theater.FileExtension : SHP_FILE_EXTENSION;
                byte[] shpData = fileManager.LoadFile(imageName + fileExtension);

                if (shpData == null)
                    continue;

                var shpFile = new ShpFile();
                shpFile.ParseFromBuffer(shpData);
                Palette palette = theaterPalette;
                if (overlayType.Wall)
                    palette = unitPalette;
                // This should be done in vanilla TS, but not in DTA
                // if (overlayType.Tiberium)
                //     palette = unitPalette;
                OverlayTextures[i] = new ObjectImage(graphicsDevice, shpFile, shpData, palette);
            }
        }

        private Random random = new Random();

        public Theater Theater { get; }

        private CCFileManager fileManager;

        private readonly Palette theaterPalette;
        private readonly Palette unitPalette;

        private List<TileImage[]> graphicsList = new List<TileImage[]>();

        public int TileCount => graphicsList.Count;

        public TileImage GetTileGraphics(int id) => graphicsList[id][random.Next(graphicsList[id].Length)];


        public ObjectImage[] TerrainObjectTextures { get; set; }
        public ObjectImage[] BuildingTextures { get; set; }
        public ObjectImage[] UnitTextures { get; set; }
        public ObjectImage[] InfantryTextures { get; set; }
        public ObjectImage[] OverlayTextures { get; set; }

        private Palette GetPaletteOrFail(string paletteFileName)
        {
            byte[] paletteData = fileManager.LoadFile(paletteFileName);
            if (paletteData == null)
                throw new KeyNotFoundException(paletteFileName + " not found from loaded MIX files!");
            return new Palette(paletteData);
        }
    }
}