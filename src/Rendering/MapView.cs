using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TSMapEditor.CCEngine;
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

        private int scrollRate = 20;

        public override void Initialize()
        {
            base.Initialize();

            renderTarget = new RenderTarget2D(GraphicsDevice,
                Map.Size.X * Constants.CellSizeX,
                Map.Size.Y * Constants.CellSizeY, false, SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        Stopwatch sw = new Stopwatch();
        public void DrawWholeMap()
        {
            sw.Restart();

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
                        DrawString("n", 0, drawPoint.ToXNAVector(), Color.Red, 0.5f);
                        continue;
                    }

                    DrawTerrainTile(row[j]);
                }
            }

            DrawOverlays();

            Texture2D debugTexture = null;

            DrawObjects();

            int a = 0;

            Renderer.PopRenderTarget();

            sw.Stop();
            Console.WriteLine("Map render time: " + sw.Elapsed.TotalMilliseconds);

            if (a == 0)
                return;

            using (var stream = File.OpenWrite(Environment.CurrentDirectory + "/texture.png"))
            {
                debugTexture.SaveAsPng(stream, debugTexture.Width, debugTexture.Height);
            }
        }

        private void DrawOverlays()
        {
            for (int i = 0; i < Map.Tiles.Length; i++)
            {
                var row = Map.Tiles[i];

                for (int j = 0; j < row.Length; j++)
                {
                    if (row[j] == null)
                        continue;

                    Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(i, j), Map.Size.X);

                    var tile = row[j];
                    if (tile.Overlay != null)
                    {
                        int overlayIndex = tile.Overlay.OverlayType.Index;
                        int frameIndex = tile.Overlay.FrameIndex;
                        DrawObject(tile.Overlay);
                    }
                }
            }
        }

        private void DrawObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>(Map.TerrainObjects);
            gameObjects.AddRange(Map.Structures);
            gameObjects.AddRange(Map.Units);
            gameObjects = gameObjects.OrderBy(s => s.GetYPositionForDrawOrder())
                .ThenBy(s => s.GetXPositionForDrawOrder()).ToList();
            gameObjects.ForEach(go => DrawObject(go));
        }

        private void DrawObject(GameObject gameObject)
        {
            Point2D drawPoint = CellMath.CellTopLeftPoint(gameObject.Position, Map.Size.X);
            
            ObjectImage graphics = null;
            Color replacementColor = Color.Red;
            string iniName = string.Empty;

            // TODO refactor this to be more object-oriented

            switch (gameObject.WhatAmI())
            {
                case RTTIType.Terrain:
                    var terrainObject = (TerrainObject)gameObject;
                    graphics = TheaterGraphics.TerrainObjectTextures[terrainObject.TerrainType.Index];
                    replacementColor = Color.Green;
                    iniName = terrainObject.TerrainType.ININame;
                    break;
                case RTTIType.Building:
                    var structure = (Structure)gameObject;
                    graphics = TheaterGraphics.BuildingTextures[structure.ObjectType.Index];
                    replacementColor = Color.Yellow;
                    iniName = structure.ObjectType.ININame;
                    break;
                case RTTIType.Unit:
                    var unit = (Unit)gameObject;
                    graphics = TheaterGraphics.UnitTextures[unit.ObjectType.Index];
                    replacementColor = Color.Red;
                    iniName = unit.ObjectType.ININame;
                    break;
                case RTTIType.Overlay:
                    var overlay = (Overlay)gameObject;
                    graphics = TheaterGraphics.OverlayTextures[overlay.OverlayType.Index];
                    replacementColor = Color.LimeGreen;
                    iniName = overlay.OverlayType.ININame;
                    break;
            }

            if (gameObject.WhatAmI() == RTTIType.Building)
            {
                var structure = (Structure)gameObject;
                int foundationX = structure.ObjectType.ArtConfig.FoundationX;
                int foundationY = structure.ObjectType.ArtConfig.FoundationY;
                if (foundationX > 0 && foundationY > 0)
                {
                    Point2D p1 = CellMath.CellTopLeftPoint(gameObject.Position, Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p2 = CellMath.CellTopLeftPoint(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p3 = CellMath.CellTopLeftPoint(new Point2D(gameObject.Position.X, gameObject.Position.Y + foundationY), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p4 = CellMath.CellTopLeftPoint(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y + foundationY), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);

                    Color foundationLineColor = new Color(128, 128, 128, 255);

                    DrawLine(p1.ToXNAVector(), p2.ToXNAVector(), foundationLineColor, 1);
                    DrawLine(p1.ToXNAVector(), p3.ToXNAVector(), foundationLineColor, 1);
                    DrawLine(p2.ToXNAVector(), p4.ToXNAVector(), foundationLineColor, 1);
                    DrawLine(p3.ToXNAVector(), p4.ToXNAVector(), foundationLineColor, 1);
                }
            }

            if (graphics == null || graphics.Frames.Length == 0)
            {
                DrawString(iniName, 1, drawPoint.ToXNAVector(), replacementColor, 1.0f);
                return;
            }

            int yDrawOffset = gameObject.GetYDrawOffset();

            int shadowFrameIndex = gameObject.GetShadowFrameIndex(graphics.Frames.Length);
            if (shadowFrameIndex > 0 && shadowFrameIndex < graphics.Frames.Length)
            {
                var shadowFrame = graphics.Frames[shadowFrameIndex];
                if (shadowFrame != null)
                {
                    var shadowTexture = shadowFrame.Texture;
                    DrawTexture(shadowTexture, new Rectangle(drawPoint.X - shadowFrame.ShapeWidth / 2 + shadowFrame.OffsetX + Constants.CellSizeX / 2,
                        drawPoint.Y - shadowFrame.ShapeHeight / 2 + shadowFrame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                        shadowTexture.Width, shadowTexture.Height), new Color(0, 0, 0, 128));
                }
            }
            
            int frameIndex = gameObject.GetFrameIndex(graphics.Frames.Length);
            var frame = graphics.Frames[frameIndex];
            if (frame == null)
                return;

            var texture = frame.Texture;
            DrawTexture(texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                texture.Width, texture.Height), Color.White);
        }

        public void DrawTerrainTile(IsoMapPack5Tile tile)
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
                
            Texture2D texture = tileImage.TMPImages[tile.SubTileIndex].Texture;
            if (texture != null)
            {
                DrawTexture(texture, new Rectangle(drawPoint.X, drawPoint.Y,
                    Constants.CellSizeX, Constants.CellSizeY), Color.White);
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

        private void DrawCursorTile()
        {
            Point cursorPoint = GetCursorPoint();
            Point2D cursorMapPoint = new Point2D(cameraTopLeftPoint.X + cursorPoint.X - Constants.CellSizeX / 2,
                cameraTopLeftPoint.Y + cursorPoint.Y - Constants.CellSizeY / 2);

            DrawStringWithShadow("Cursor coord: " + cursorMapPoint.X + ", " + cursorMapPoint.Y, 0, new Vector2(0f, 0f), Color.White);

            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(cursorMapPoint, Map.Size);

            DrawStringWithShadow(tileCoords.X + ", " + tileCoords.Y, 0, new Vector2(0f, 20f), Color.White);

            if (tileCoords.X >= 1 && tileCoords.Y >= 1 && tileCoords.Y < Map.Tiles.Length && tileCoords.X < Map.Tiles[tileCoords.Y].Length)
            {
                var tile = Map.Tiles[tileCoords.Y][tileCoords.X];

                if (tile == null)
                {
                    DrawString("Null tile", 0, new Vector2(0f, 40f), Color.White);
                }
                else
                {
                    Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(tile.X, tile.Y), Map.Size.X);

                    FillRectangle(new Rectangle(drawPoint.X - cameraTopLeftPoint.X,
                        drawPoint.Y - cameraTopLeftPoint.Y,
                        Constants.CellSizeX, Constants.CellSizeY),
                        new Color(128, 128, 128, 128));

                    TileImage tileGraphics = TheaterGraphics.GetTileGraphics(tile.TileIndex);
                    TileSet tileSet = TheaterGraphics.Theater.TileSets[tileGraphics.TileSetId];
                    DrawStringWithShadow("TileSet: " + tileSet.SetName + " (" + tileGraphics.TileSetId + ")", 0,
                        new Vector2(0f, 40f), Color.White);
                    DrawStringWithShadow("Tile ID: " + tileGraphics.TileIndex, 0, new Vector2(0f, 60f), Color.White);
                    DrawStringWithShadow("Sub-tile ID: " + tile.SubTileIndex, 0, new Vector2(0f, 80f), Color.White);
                }
            }
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

            DrawCursorTile();

            base.Draw(gameTime);
        }
    }
}
