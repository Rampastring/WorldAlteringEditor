using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Misc
{
    public class TerrainImporter
    {
        public class Configuration
        {
            public Dictionary<Color, int> Mapping { get; set; }
            public int Default { get; set; }

            public Configuration() {}
            public Configuration(string fileName)
            {
                var tileTypes = new Dictionary<string, int>();
                var colorTypes = new Dictionary<string, Color>();

                var iniFile = new IniFile(fileName);
                var sTileTypes = iniFile.GetSection("Tiles");
                var sColorTypes = iniFile.GetSection("Colors");
                var sMapping = iniFile.GetSection("Mapping");

                foreach (var tileType in sTileTypes.Keys)
                    tileTypes.Add(tileType.Key, int.Parse(tileType.Value));
                foreach (var colorType in sColorTypes.Keys)
                    colorTypes.Add(colorType.Key, Helpers.ColorFromString(colorType.Value));

                Mapping = new Dictionary<Color, int>();
                foreach (var mapping in sMapping.Keys)
                    Mapping.Add(colorTypes[mapping.Key], tileTypes[mapping.Value]);

                Default = tileTypes["Default"];
            }

            public Dictionary<Color, ITileImage> ReadTheater(ITheater theater)
            {
                Dictionary<Color, ITileImage> mapping = new Dictionary<Color, ITileImage>();
                foreach (var pair in Mapping)
                    mapping.Add(pair.Key, theater.GetTile(pair.Value));
                return mapping;
            }
        }

        public Configuration Current { get; set; }

        public TerrainImporter(Configuration configuration)
        {
            Current = configuration;
        }
        
        public static Color GetPixel(Color[] colors, int x, int y, int width)
        {
            return colors[x + (y * width)];
        }
        public static Color[] GetPixels(Texture2D texture)
        {
            Color[] colors1D = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(colors1D);
            return colors1D;
        }

        public void Import(Map map, Texture2D terrainMap, Texture2D heightMap, byte basicLevel)
        {
            Dictionary<Color, ITileImage> mapping = Current.ReadTheater(map.TheaterInstance);
            var defaultTileImg = map.TheaterInstance.GetTile(Current.Default);

            short w = (short) map.Size.X;
            short h = (short) map.Size.Y;
            var tiStepX = terrainMap == null ? -1 : Math.Round((double)terrainMap.Width / w);
            var tiStepY = terrainMap == null ? -1 : Math.Round((double)terrainMap.Height / h);
            var hmStepX = heightMap == null ? -1 : Math.Round((double)heightMap.Width / w);
            var hmStepY = heightMap == null ? -1 : Math.Round((double)heightMap.Height / h);
            var terrainData = terrainMap == null ? null : GetPixels(terrainMap);
            var heightData = heightMap == null ? null : GetPixels(heightMap);

            var offX = 0;
            var offY = ((w + h) / -2);
            
            List<MapTile> tiles = new List<MapTile>();
            var createTile = (int x, int y, int bx, int by, int otx, int oty) =>
            {
                var tileImg = defaultTileImg;
                var height = basicLevel;
                if (terrainMap != null)
                {
                    var tiX = (int)Math.Clamp(tiStepX * (x + bx), 0, terrainMap.Width - 1);
                    var tiY = (int)Math.Clamp(tiStepY * (y + by), 0, terrainMap.Height - 1);
                    var terrainColor = GetPixel(terrainData, tiX, tiY, terrainMap.Width);
                    tileImg = mapping.ContainsKey(terrainColor) ? mapping[terrainColor] : defaultTileImg;
                }
                if (heightMap != null)
                {
                    var hmX = (int)Math.Clamp(hmStepX * (x + bx), 0, heightMap.Width - 1);
                    var hmY = (int)Math.Clamp(hmStepY * (y + by), 0, heightMap.Height - 1);
                    var heightColor = GetPixel(heightData, hmX, hmY, heightMap.Width);
                    height += (byte)(((short)heightColor.R + heightColor.G + heightColor.B) / 3);
                }

                {
                    var tileX = 1 + x + y + offX;
                    var tileY = 1 + h + w - x + y + offY;
                    var tile = new MapTile();

                    tile.Level = height;
                    tile.ChangeTileIndex(tileImg.TileID, 0);

                    tile.X = (short)Math.Round((double)tileX + otx); tile.Y = (short)Math.Round((double)tileY + oty);
                    tiles.Add(tile);
                }
            };

            for (int y = 0; y < map.Size.Y; y++)
                for (int x = 0; x < map.Size.X; x++)
                {
                    // here there is a problem
                    // if create tile only for (x, y) then it became like chessboard:
                    // line (x+1, y) will be skipped.
                    // the easiest way just create tile line for (x+1, y) seperately.
                    //
                    // About offset. Some experiments
                    // *50x*61 <=> *5x*6
                    // 100x122 <=> 11x11
                    // 150x183 <=> 18x17
                    // 200x244 <=> 23x22
                    // The conclusion:
                    // otx = 0 - because the y component can be found in both formulas
                    // oty = h / 10
                    var otx = 0;
                    var oty = -((h / 6));
                    createTile(x, y, 0, 0, otx, oty);
                    createTile(x, y, 1, 0, 1 + otx, oty);
                }

            map.SetTileData(tiles);
        }
    }
}
