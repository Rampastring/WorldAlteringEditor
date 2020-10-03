using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Rendering
{
    public class MapView : XNAControl
    {
        public MapView(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            Map = map;
            TheaterGraphics = theaterGraphics;
        }

        public Map Map { get; }
        public TheaterGraphics TheaterGraphics { get; }

        private RenderTarget2D renderTarget;

        private bool mapInvalidated = true;
        private Point2D cameraTopLeftPoint = new Point2D(0, 0);

        private int scrollRate = 10;

        public override void Initialize()
        {
            base.Initialize();

            renderTarget = new RenderTarget2D(GraphicsDevice,
                Map.Size.X * Constants.CellSizeX,
                Map.Size.Y * Constants.CellSizeY, false, SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public void DrawWholeMap()
        {
            Renderer.PushRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Black);

            for (int i = 0; i < Map.Tiles.Length; i++)
            {
                var row = Map.Tiles[i];

                for (int j = 0; j < row.Length; j++)
                {
                    Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(i, j), Map.Size.X);
                    if (row[j] == null)
                    {
                        DrawString("n", 0, new Vector2(drawPoint.X, drawPoint.Y), Color.Red, 0.5f);
                        continue;
                    }

                    if (row[j] == null)
                        continue;

                    DrawTerrain(row[j]);
                }
            }

            Renderer.PopRenderTarget();
        }

        public void DrawTerrain(IsoMapPack5Tile tile)
        {
            Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(tile.X, tile.Y), Map.Size.X);
            

            if (tile.TileIndex >= TheaterGraphics.TileCount)
                return;

            TileImage tileImage = TheaterGraphics.GetTileGraphics(tile.TileIndex);
            if (tile.SubTileIndex >= tileImage.TMPImages.Length)
            {
                DrawString(tile.SubTileIndex.ToString(), 0, new Vector2(drawPoint.X, drawPoint.Y), Color.Red);
                return;
            }
                

            int a = 0;
            
            Texture2D texture = tileImage.TMPImages[tile.SubTileIndex].Texture;
            if (texture != null)
            {
                DrawTexture(texture, new Rectangle(drawPoint.X, drawPoint.Y,
                    Constants.CellSizeX, Constants.CellSizeY), Color.White);
            }

            if (a == 0)
                return;

            using (var stream = File.OpenWrite(Environment.CurrentDirectory + "/texture.png"))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Left))
                cameraTopLeftPoint += new Point2D(-scrollRate, 0);
            else if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Right))
                cameraTopLeftPoint += new Point2D(scrollRate, 0);

            if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Up))
                cameraTopLeftPoint += new Point2D(0, -scrollRate);
            else if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Down))
                cameraTopLeftPoint += new Point2D(0, scrollRate);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (mapInvalidated)
            {
                DrawWholeMap();
                mapInvalidated = false;
            }

            DrawTexture(renderTarget, new Rectangle(cameraTopLeftPoint.X, cameraTopLeftPoint.Y,
                Width, Height), new Rectangle(0, 0, Width, Height), Color.White);

            base.Draw(gameTime);
        }
    }
}
