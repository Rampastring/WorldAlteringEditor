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
using TSMapEditor.UI;

namespace TSMapEditor.Rendering
{
    public interface ICursorActionTarget
    {
        Map Map { get; }
        TheaterGraphics TheaterGraphics { get; }
        WindowManager WindowManager { get; }
        void AddRefreshPoint(Point2D point);
    }

    public class MapView : XNAControl, ICursorActionTarget
    {
        public MapView(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            Map = map;
            TheaterGraphics = theaterGraphics;
        }

        public Map Map { get; }
        public TheaterGraphics TheaterGraphics { get; }

        public CursorAction CursorAction { get; set; }

        private RenderTarget2D mapRenderTarget;

        private MapTile tileUnderCursor;

        private bool mapInvalidated = true;
        private Point2D cameraTopLeftPoint = new Point2D(0, 0);

        private int scrollRate = 20;

        private List<Point2D> refreshes = new List<Point2D>();

        public void AddRefreshPoint(Point2D point)
        {
            refreshes.Add(point);
        }

        public override void Initialize()
        {
            base.Initialize();

            mapRenderTarget = new RenderTarget2D(GraphicsDevice,
                Map.Size.X * Constants.CellSizeX,
                Map.Size.Y * Constants.CellSizeY, false, SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
        }

        Stopwatch sw = new Stopwatch();
        public void DrawWholeMap()
        {
            sw.Restart();

            Renderer.PushRenderTarget(mapRenderTarget);
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
            gameObjects.AddRange(Map.Infantry);
            gameObjects = gameObjects.OrderBy(s => s.GetYPositionForDrawOrder())
                .ThenBy(s => s.GetXPositionForDrawOrder()).ToList();
            gameObjects.ForEach(go => DrawObject(go));
        }

        private void RefreshOverArea(Point2D center)
        {
            var tilesToRedraw = new List<MapTile>();
            var objectsToRedraw = new List<GameObject>();
            var cellCoords = CellMath.CellCoordsFromPixelCoords(center, Map.Size);
            const int tileRefreshArea = 10;
            const int objectRefreshArea = 20;
            for (int y = -tileRefreshArea; y <= tileRefreshArea; y++)
            {
                for (int x = -tileRefreshArea; x <= tileRefreshArea; x++)
                {
                    var tile = Map.Tiles[cellCoords.Y + y][cellCoords.X + x];
                    if (tile == null)
                        continue;

                    tilesToRedraw.Add(tile);
                }
            }

            for (int y = -objectRefreshArea; y <= objectRefreshArea; y++)
            {
                for (int x = -objectRefreshArea; x <= objectRefreshArea; x++)
                {
                    var tile = Map.Tiles[cellCoords.Y + y][cellCoords.X + x];
                    if (tile == null)
                        continue;

                    for (int i = 0; i < tile.Infantry.Length; i++)
                    {
                        if (tile.Infantry[i] != null)
                            objectsToRedraw.Add(tile.Infantry[i]);
                    }

                    if (tile.Vehicle != null)
                        objectsToRedraw.Add(tile.Vehicle);

                    if (tile.Structure != null)
                        objectsToRedraw.Add(tile.Structure);

                    if (tile.TerrainObject != null)
                        objectsToRedraw.Add(tile.TerrainObject);
                }
            }

            objectsToRedraw = objectsToRedraw.OrderBy(s => s.GetYPositionForDrawOrder())
                .ThenBy(s => s.GetXPositionForDrawOrder()).ToList();

            Renderer.PushRenderTarget(mapRenderTarget);
            tilesToRedraw.ForEach(t => DrawTerrainTile(t));
            tilesToRedraw.ForEach(t =>
            {
                if (t.Overlay != null)
                    DrawObject(t.Overlay);
            });
            objectsToRedraw.ForEach(o => DrawObject(o));
            Renderer.PopRenderTarget();
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
                case RTTIType.Infantry:
                    var infantry = (Infantry)gameObject;
                    graphics = TheaterGraphics.InfantryTextures[infantry.ObjectType.Index];
                    replacementColor = Color.Teal;
                    iniName = infantry.ObjectType.ININame;
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

            // int shadowFrameIndex = gameObject.GetShadowFrameIndex(graphics.Frames.Length);
            // if (shadowFrameIndex > 0 && shadowFrameIndex < graphics.Frames.Length)
            // {
            //     var shadowFrame = graphics.Frames[shadowFrameIndex];
            //     if (shadowFrame != null)
            //     {
            //         var shadowTexture = shadowFrame.Texture;
            //         DrawTexture(shadowTexture, new Rectangle(drawPoint.X - shadowFrame.ShapeWidth / 2 + shadowFrame.OffsetX + Constants.CellSizeX / 2,
            //             drawPoint.Y - shadowFrame.ShapeHeight / 2 + shadowFrame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
            //             shadowTexture.Width, shadowTexture.Height), new Color(0, 0, 0, 128));
            //     }
            // }
            
            int frameIndex = gameObject.GetFrameIndex(graphics.Frames.Length);
            var frame = graphics.Frames[frameIndex];
            if (frame == null)
                return;

            var texture = frame.Texture;
            DrawTexture(texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                texture.Width, texture.Height), Color.White);

            if (gameObject.WhatAmI() == RTTIType.Unit)
            {
                var unit = (Unit)gameObject;
                if (unit.UnitType.Turret)
                {
                    int turretFrameIndex = unit.GetTurretFrameIndex();
                    if (turretFrameIndex > -1 && turretFrameIndex < graphics.Frames.Length)
                    {
                        frame = graphics.Frames[turretFrameIndex];
                        if (frame == null)
                            return;
                        texture = frame.Texture;
                        DrawTexture(texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                            drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                            texture.Width, texture.Height), Color.White);
                    }
                }
            }
        }

        public void DrawTerrainTile(MapTile tile)
        {
            Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(tile.X, tile.Y), Map.Size.X);
            
            if (tile.TileIndex >= TheaterGraphics.TileCount)
                return;

            if (tile.TileImage == null)
                tile.TileImage = TheaterGraphics.GetTileGraphics(tile.TileIndex);

            var tileImage = tile.TileImage;
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

        private Point2D GetCursorMapPoint()
        {
            Point cursorPoint = GetCursorPoint();
            Point2D cursorMapPoint = new Point2D(cameraTopLeftPoint.X + cursorPoint.X - Constants.CellSizeX / 2,
                    cameraTopLeftPoint.Y + cursorPoint.Y - Constants.CellSizeY / 2);
            return cursorMapPoint;
        }

        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            if (!IsActive)
                return;

            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(GetCursorMapPoint(), Map.Size);

            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.A)
            {
                if (tileCoords.X >= 1 && tileCoords.Y >= 1 && tileCoords.Y < Map.Tiles.Length && tileCoords.X < Map.Tiles[tileCoords.Y].Length)
                {
                    var tile = Map.Tiles[tileCoords.Y][tileCoords.X];
                    if (tile == null)
                        return;

                    var unit = Map.Units.Find(u => u.Position.X == tile.X && u.Position.Y == tile.Y);
                    if (unit != null)
                    {
                        int facing = unit.Facing;
                        facing += 8;
                        facing = facing % 256;
                        unit.Facing = (byte)facing;
                        refreshes.Add(CellMath.CellTopLeftPoint(unit.Position, Map.Size.X));
                    }
                }

                return;
            }

            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.Delete)
            {
                DeleteObjectFromTile(tileCoords);
                refreshes.Add(CellMath.CellTopLeftPoint(tileCoords, Map.Size.X));
            }
        }

        private void DrawCursorTile()
        {
            if (!IsActive)
                return;

            Point2D cursorMapPoint = GetCursorMapPoint();

            DrawStringWithShadow("Cursor coord: " + cursorMapPoint.X + ", " + cursorMapPoint.Y, 0, new Vector2(0f, 0f), Color.White);

            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(cursorMapPoint, Map.Size);

            DrawStringWithShadow(tileCoords.X + ", " + tileCoords.Y, 0, new Vector2(0f, 20f), Color.White);

            if (tileCoords.X >= 1 && tileCoords.Y >= 1 && tileCoords.Y < Map.Tiles.Length && tileCoords.X < Map.Tiles[tileCoords.Y].Length)
            {
                var tile = Map.Tiles[tileCoords.Y][tileCoords.X];
                tileUnderCursor = tile;

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
            else
            {
                tileUnderCursor = null;
            }
        }

        public void DeleteObjectFromTile(Point2D cellCoords)
        {
            var tile = Map.GetTile(cellCoords.X, cellCoords.Y);
            if (tile == null)
                return;

            for (int i = 0; i < tile.Infantry.Length; i++)
            {
                if (tile.Infantry[i] != null)
                {
                    Map.Infantry.Remove(tile.Infantry[i]);
                    tile.Infantry[i] = null;
                    return;
                }
            }

            if (tile.Aircraft != null)
            {
                Map.Aircraft.Remove(tile.Aircraft);
                tile.Aircraft = null;
                return;
            }

            if (tile.Vehicle != null)
            {
                Map.Units.Remove(tile.Vehicle);
                tile.Vehicle = null;
                return;
            }

            if (tile.Structure != null)
            {
                throw new NotImplementedException();
            }

            if (tile.TerrainObject != null)
            {
                Map.TerrainObjects.Remove(tile.TerrainObject);
                tile.TerrainObject = null;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (mapInvalidated)
            {
                DrawWholeMap();
                mapInvalidated = false;
            }

            foreach (var refresh in refreshes)
            {
                RefreshOverArea(refresh);
            }
            refreshes.Clear();

            DrawTexture(mapRenderTarget, new Rectangle(cameraTopLeftPoint.X, cameraTopLeftPoint.Y,
                Width, Height), new Rectangle(0, 0, Width, Height), Color.White);

            if (tileUnderCursor != null && CursorAction != null)
            {
                Point2D cursorMapPoint = GetCursorMapPoint();
                CursorAction.DrawPreview(
                    CellMath.CellTopLeftPoint(new Point2D(tileUnderCursor.X, tileUnderCursor.Y), Map.Size.X) - cameraTopLeftPoint,
                    this);
            }

            DrawCursorTile();

            base.Draw(gameTime);
        }
    }
}
