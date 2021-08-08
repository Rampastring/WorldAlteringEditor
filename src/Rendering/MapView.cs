using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Settings;
using TSMapEditor.UI;
using TSMapEditor.UI.CursorActions;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// An interface for an object that mutations use to interact with the map.
    /// </summary>
    public interface IMutationTarget
    {
        Map Map { get; }
        TheaterGraphics TheaterGraphics { get; }
        void AddRefreshPoint(Point2D point, int size = 10);
        void InvalidateMap();
        House ObjectOwner { get; }
        BrushSize BrushSize { get; }
        Randomizer Randomizer { get; }
        bool AutoLATEnabled { get; }
    }

    /// <summary>
    /// An interface for an object that cursor actions use to interact with the map.
    /// </summary>
    public interface ICursorActionTarget
    {
        Map Map { get; }
        TheaterGraphics TheaterGraphics { get; }
        WindowManager WindowManager { get; }
        void AddRefreshPoint(Point2D point, int size = 10);
        void InvalidateMap();
        MutationManager MutationManager { get; }
        IMutationTarget MutationTarget { get; }
        BrushSize BrushSize { get; }
        Randomizer Randomizer { get; }
        bool AutoLATEnabled { get; }
        List<CopiedTerrainData> CopiedTerrainData { get; }
    }

    struct RefreshPoint
    {
        public Point2D CellCoords;
        public int Size;

        public RefreshPoint(Point2D cellCoords, int size)
        {
            CellCoords = cellCoords;
            Size = size;
        }
    }

    public class MapView : XNAControl, ICursorActionTarget, IMutationTarget
    {
        public MapView(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics, EditorState editorState, MutationManager mutationManager) : base(windowManager)
        {
            EditorState = editorState;
            Map = map;
            TheaterGraphics = theaterGraphics;
            MutationManager = mutationManager;
        }

        public EditorState EditorState { get; }
        public Map Map { get; }
        public TheaterGraphics TheaterGraphics { get; }
        public MutationManager MutationManager { get; }

        public IMutationTarget MutationTarget => this;
        public House ObjectOwner => EditorState.ObjectOwner;
        public BrushSize BrushSize => EditorState.BrushSize;
        public Randomizer Randomizer => EditorState.Randomizer;
        public bool AutoLATEnabled => EditorState.AutoLATEnabled;
        public List<CopiedTerrainData> CopiedTerrainData => EditorState.CopiedTerrainData;

        public TileInfoDisplay TileInfoDisplay { get; set; }
        
        public CursorAction CursorAction
        {
            get => EditorState.CursorAction;
            set => EditorState.CursorAction = value;
        }

        private RenderTarget2D mapRenderTarget;
        private RenderTarget2D objectRenderTarget;

        private MapTile tileUnderCursor;
        private MapTile lastTileUnderCursor;

        private bool mapInvalidated = true;
        private Point2D cameraTopLeftPoint = new Point2D(0, 0);

        private int scrollRate;

        private bool isDraggingObject = false;
        private GameObject draggedObject = null;

        // For right-click scrolling
        private bool isRightClickScrolling = false;
        private Point rightClickScrollInitPos = new Point(-1, -1);
        private Vector2 cameraFloatTopLeftPoint = new Vector2(-1, -1);

        /// <summary>
        /// A list of cells that have been invalidated.
        /// Areas around these cells are to be redrawn.
        /// </summary>
        private List<RefreshPoint> refreshes = new List<RefreshPoint>();

        private CopyTerrainCursorAction copyTerrainCursorAction;
        private PasteTerrainCursorAction pasteTerrainCursorAction;

        public void AddRefreshPoint(Point2D point, int size = 10)
        {
            if (!mapInvalidated)
                refreshes.Add(new RefreshPoint(point, size));
        }

        public void InvalidateMap()
        {
            refreshes.Clear();
            mapInvalidated = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            mapRenderTarget = CreateFullMapRenderTarget();
            objectRenderTarget = CreateFullMapRenderTarget();

            scrollRate = UserSettings.Instance.ScrollRate;

            EditorState.CursorActionChanged += EditorState_CursorActionChanged;

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
            KeyboardCommands.Instance.FrameworkMode.Triggered += FrameworkMode_Triggered;

            copyTerrainCursorAction = new CopyTerrainCursorAction(this);
            pasteTerrainCursorAction = new PasteTerrainCursorAction(this);
        }

        private void FrameworkMode_Triggered(object sender, EventArgs e)
        {
            EditorState.IsMarbleMadness = !EditorState.IsMarbleMadness;
            mapInvalidated = true;
        }

        private void EditorState_CursorActionChanged(object sender, EventArgs e)
        {
            lastTileUnderCursor = null;
        }

        private RenderTarget2D CreateFullMapRenderTarget()
        {
           return new RenderTarget2D(GraphicsDevice,
               Map.Size.X * Constants.CellSizeX,
               Map.Size.Y * Constants.CellSizeY + Constants.CellSizeY / 2, false, SurfaceFormat.Color,
               DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
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
                        DrawString("n", 0, drawPoint.ToXNAVector() + new Vector2(Constants.CellSizeX / 2, Constants.CellSizeY / 2), Color.Red, 0.5f);
                        continue;
                    }

                    DrawTerrainTile(row[j]);
                }
            }

            DrawOverlays();

            DrawObjects();
            DrawCellTags();
            DrawWaypoints();
            DrawMapBorder();

            Renderer.PopRenderTarget();

            sw.Stop();
            Console.WriteLine("Map render time: " + sw.Elapsed.TotalMilliseconds);
        }

        private void DrawOverlays()
        {
            Map.DoForAllValidTiles(t =>
            {
                if (t.Overlay != null)
                    DrawObject(t.Overlay);
            });
        }

        private void DrawWaypoints()
        {
            Map.DoForAllValidTiles(t =>
            {
                if (t.Waypoint != null)
                    DrawWaypoint(t.Waypoint);
            });
        }

        private void DrawCellTags()
        {
            Map.DoForAllValidTiles(t =>
            {
                if (t.CellTag != null)
                    DrawCellTag(t.CellTag);
            });
        }

        private void DrawObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>(Map.TerrainObjects);
            gameObjects.AddRange(Map.Structures);
            gameObjects.AddRange(Map.Aircraft);
            gameObjects.AddRange(Map.Units);
            gameObjects.AddRange(Map.Infantry);
            gameObjects = gameObjects.OrderBy(s => s.GetYPositionForDrawOrder())
                .ThenBy(s => s.GetXPositionForDrawOrder()).ToList();
            gameObjects.ForEach(go => DrawObject(go));
        }

        private void RefreshOverArea(RefreshPoint refreshPoint)
        {
            var tilesToRedraw = new List<MapTile>();
            var waypointsToRedraw = new List<Waypoint>();
            var cellTagsToRedraw = new List<CellTag>();
            var objectsToRedraw = new List<GameObject>();
            //const int tileRefreshArea = 10;
            int objectRefreshArea = refreshPoint.Size + 3;

            for (int y = -refreshPoint.Size; y <= refreshPoint.Size; y++)
            {
                for (int x = -refreshPoint.Size; x <= refreshPoint.Size; x++)
                {
                    var tile = Map.GetTile(refreshPoint.CellCoords.X + x, refreshPoint.CellCoords.Y + y);
                    if (tile == null)
                        continue;

                    tilesToRedraw.Add(tile);

                    if (tile.Waypoint != null)
                        waypointsToRedraw.Add(tile.Waypoint);

                    if (tile.CellTag != null)
                        cellTagsToRedraw.Add(tile.CellTag);
                }
            }

            for (int y = -objectRefreshArea; y <= objectRefreshArea; y++)
            {
                for (int x = -objectRefreshArea; x <= objectRefreshArea; x++)
                {
                    var tile = Map.GetTile(refreshPoint.CellCoords.X + x, refreshPoint.CellCoords.Y + y);
                    if (tile == null)
                        continue;

                    for (int i = 0; i < tile.Infantry.Length; i++)
                    {
                        if (tile.Infantry[i] != null)
                            objectsToRedraw.Add(tile.Infantry[i]);
                    }

                    if (tile.Aircraft != null)
                        objectsToRedraw.Add(tile.Aircraft);

                    if (tile.Vehicle != null)
                        objectsToRedraw.Add(tile.Vehicle);

                    if (tile.Structure != null && !objectsToRedraw.Contains(tile.Structure))
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
            cellTagsToRedraw.ForEach(c => DrawCellTag(c));
            waypointsToRedraw.ForEach(w => DrawWaypoint(w));
            DrawMapBorder();
            Renderer.PopRenderTarget();
        }

        private void DrawObject(GameObject gameObject)
        {
            Point2D drawPoint = CellMath.CellTopLeftPoint(gameObject.Position, Map.Size.X);

            ObjectImage graphics = null;
            Color replacementColor = Color.Red;
            Color remapColor = Color.White;
            string iniName = string.Empty;

            // TODO refactor this to be more object-oriented

            switch (gameObject.WhatAmI())
            {
                case RTTIType.Aircraft:
                    var aircraft = (Aircraft)gameObject;
                    replacementColor = Color.HotPink;
                    iniName = aircraft.ObjectType.ININame;
                    break;
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
                    remapColor = structure.ObjectType.ArtConfig.Remapable ? structure.Owner.XNAColor : remapColor;
                    break;
                case RTTIType.Unit:
                    var unit = (Unit)gameObject;
                    graphics = TheaterGraphics.UnitTextures[unit.ObjectType.Index];
                    replacementColor = Color.Red;
                    iniName = unit.ObjectType.ININame;
                    remapColor = unit.ObjectType.ArtConfig.Remapable ? unit.Owner.XNAColor : remapColor;
                    break;
                case RTTIType.Infantry:
                    var infantry = (Infantry)gameObject;
                    graphics = TheaterGraphics.InfantryTextures[infantry.ObjectType.Index];
                    replacementColor = Color.Teal;
                    iniName = infantry.ObjectType.ININame;
                    remapColor = infantry.ObjectType.ArtConfig.Remapable ? infantry.Owner.XNAColor : remapColor;
                    switch (infantry.SubCell)
                    {
                        case SubCell.Top:
                            drawPoint += new Point2D(0, Constants.CellSizeY / -4);
                            break;
                        case SubCell.Bottom:
                            drawPoint += new Point2D(0, Constants.CellSizeY / 4);
                            break;
                        case SubCell.Left:
                            drawPoint += new Point2D(Constants.CellSizeX / -4, 0);
                            break;
                        case SubCell.Right:
                            drawPoint += new Point2D(Constants.CellSizeX / 4, 0);
                            break;
                        case SubCell.Center:
                        default:
                            break;
                    }
                    break;
                case RTTIType.Overlay:
                    var overlay = (Overlay)gameObject;
                    if (overlay.OverlayType == null)
                        return;
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
                DrawStringWithShadow(iniName, 1, drawPoint.ToXNAVector(), replacementColor, 1.0f);
                return;
            }

            int yDrawOffset = gameObject.GetYDrawOffset();

            if (Constants.DrawShadows)
            {
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
            }
            
            int frameIndex = gameObject.GetFrameIndex(graphics.Frames.Length);
            if (frameIndex >= graphics.Frames.Length)
                return;

            var frame = graphics.Frames[frameIndex];
            if (frame == null)
                return;

            var texture = frame.Texture;
            DrawTexture(texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                texture.Width, texture.Height), Constants.HQRemap ? Color.White : remapColor);

            if (Constants.HQRemap && graphics.RemapFrames != null)
            {
                DrawTexture(graphics.RemapFrames[frameIndex].Texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                    drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                    texture.Width, texture.Height), remapColor);
            }

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
                            texture.Width, texture.Height), Constants.HQRemap ? Color.White : remapColor);

                        if (Constants.HQRemap && graphics.RemapFrames != null)
                        {
                            DrawTexture(graphics.RemapFrames[turretFrameIndex].Texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                                drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                                texture.Width, texture.Height), remapColor);
                        }
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

            TileImage tileImage;
            int subTileIndex;
            if (tile.PreviewTileImage != null)
            {
                tileImage = tile.PreviewTileImage;
                subTileIndex = tile.PreviewSubTileIndex;
            }
            else
            {
                tileImage = tile.TileImage;
                subTileIndex = tile.SubTileIndex;
            }

            // MM support
            if (EditorState.IsMarbleMadness)
                tileImage = TheaterGraphics.GetMarbleMadnessTileGraphics(tileImage.TileID);

            if (subTileIndex >= tileImage.TMPImages.Length)
            {
                DrawString(subTileIndex.ToString(), 0, new Vector2(drawPoint.X, drawPoint.Y), Color.Red);
                return;
            }
            
            Texture2D texture = tileImage.TMPImages[subTileIndex].Texture;
            if (texture != null)
            {
                DrawTexture(texture, new Rectangle(drawPoint.X, drawPoint.Y,
                    Constants.CellSizeX, Constants.CellSizeY), Color.White);
            }
        }

        private void DrawWaypoint(Waypoint waypoint)
        {
            const int waypointBorderOffsetX = 8;
            const int waypointBorderOffsetY = 4;
            const int textOffset = 3;

            Point2D drawPoint = CellMath.CellTopLeftPoint(waypoint.Position, Map.Size.X);

            var rect = new Rectangle(drawPoint.X + waypointBorderOffsetX,
                drawPoint.Y + waypointBorderOffsetY,
                Constants.CellSizeX - (waypointBorderOffsetX * 2),
                Constants.CellSizeY - (waypointBorderOffsetY * 2));

            Color waypointColor = Color.Fuchsia;

            FillRectangle(rect, new Color(0, 0, 0, 128));
            DrawRectangle(rect, waypointColor);
            DrawStringWithShadow(waypoint.Identifier.ToString(),
                Constants.UIDefaultFont, 
                new Vector2(rect.X + textOffset, rect.Y),
                waypointColor);
        }

        private void DrawCellTag(CellTag cellTag)
        {
            const int cellTagBorderOffsetX = 12;
            const int cellTagBorderOffsetY = 4;

            Point2D drawPoint = CellMath.CellTopLeftPoint(cellTag.Position, Map.Size.X);

            var rect = new Rectangle(drawPoint.X + cellTagBorderOffsetX,
                drawPoint.Y + cellTagBorderOffsetY,
                Constants.CellSizeX - (cellTagBorderOffsetX * 2),
                Constants.CellSizeY - (cellTagBorderOffsetY * 2));

            FillRectangle(rect, new Color(128, 0, 0, 128));
            DrawRectangle(rect, Color.Yellow);
        }

        private void DrawMapBorder()
        {
            // TODO this algorithm seems correct for the x-axis, but it's wrong for the y-axis
            int x = Map.LocalSize.X * Constants.CellSizeX;
            int y = Map.LocalSize.Y * (Constants.CellSizeY * 3) / 4;
            int width = Map.LocalSize.Width * Constants.CellSizeX;
            int bottom = (Map.Size.Y - (Map.Size.Y - Map.LocalSize.Y - Map.LocalSize.Height)) * Constants.CellSizeY;
            int height = bottom - y;

            DrawRectangle(new Rectangle(x, y, width, height), Color.Blue, 4);
        }

        public override void OnMouseOnControl()
        {
            if (CursorAction == null && isDraggingObject)
            {
                if (!Cursor.LeftDown)
                {
                    isDraggingObject = false;

                    if (tileUnderCursor != null && tileUnderCursor.CoordsToPoint() != draggedObject.Position)
                    {
                        if (Map.CanMoveObject(draggedObject, tileUnderCursor.CoordsToPoint()))
                        {
                            var mutation = new MoveObjectMutation(MutationTarget, draggedObject, tileUnderCursor.CoordsToPoint());
                            MutationManager.PerformMutation(mutation);
                        }
                    }
                }
            }

            if (isRightClickScrolling)
            {
                if (Cursor.RightDown)
                {
                    var newCursorPosition = GetCursorPoint();
                    var result = newCursorPosition - rightClickScrollInitPos;
                    float rightClickScrollRate = scrollRate / 64f;

                    cameraFloatTopLeftPoint = new Vector2(cameraFloatTopLeftPoint.X + result.X * rightClickScrollRate,
                        cameraFloatTopLeftPoint.Y + result.Y * rightClickScrollRate);

                    cameraTopLeftPoint = new Point2D((int)cameraFloatTopLeftPoint.X,
                        (int)cameraFloatTopLeftPoint.Y);
                }
            }

            base.OnMouseOnControl();
        }

        public override void OnMouseLeftDown()
        {
            if (CursorAction == null && tileUnderCursor != null)
            {
                draggedObject = tileUnderCursor.GetObject();

                if (draggedObject != null)
                    isDraggingObject = true;
            }

            base.OnMouseLeftDown();
        }

        public override void OnMouseMove()
        {
            base.OnMouseMove();

            if (tileUnderCursor != null && CursorAction != null)
            {
                if (Cursor.LeftDown && lastTileUnderCursor != tileUnderCursor)
                {
                    CursorAction.LeftDown(tileUnderCursor.CoordsToPoint());
                    lastTileUnderCursor = tileUnderCursor;
                }
            }

            // Right-click scrolling
            if (Cursor.RightDown)
            {
                if (!isRightClickScrolling)
                {
                    isRightClickScrolling = true;
                    rightClickScrollInitPos = GetCursorPoint();
                    cameraFloatTopLeftPoint = cameraTopLeftPoint.ToXNAVector();
                }
            }
        }

        public override void OnLeftClick()
        {
            if (tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.LeftClick(tileUnderCursor.CoordsToPoint());
            }

            base.OnLeftClick();
        }

        public override void OnRightClick()
        {
            if (CursorAction != null && !isRightClickScrolling)
            {
                CursorAction = null;
            }

            isRightClickScrolling = false;

            base.OnRightClick();
        }

        public override void Update(GameTime gameTime)
        {
            // Make scroll rate independent of FPS
            // Scroll rate is designed for 60 FPS
            // 1000 ms (1 second) divided by 60 frames =~ 16.667 ms / frame
            int scrollRate = (int)(this.scrollRate * (gameTime.ElapsedGameTime.TotalMilliseconds / 16.667));

            if (IsActive && !(WindowManager.SelectedControl is XNATextBox))
            {
                if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Left))
                    cameraTopLeftPoint += new Point2D(-scrollRate, 0);
                else if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Right))
                    cameraTopLeftPoint += new Point2D(scrollRate, 0);

                if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Up))
                    cameraTopLeftPoint += new Point2D(0, -scrollRate);
                else if (Keyboard.IsKeyHeldDown(Microsoft.Xna.Framework.Input.Keys.Down))
                    cameraTopLeftPoint += new Point2D(0, scrollRate);
            }

            Point2D cursorMapPoint = GetCursorMapPoint();
            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(cursorMapPoint, Map.Size);
            var tile = Map.GetTile(tileCoords.X, tileCoords.Y);
            tileUnderCursor = tile;
            TileInfoDisplay.MapTile = tile;

            if (IsActive && tileUnderCursor != null)
            {
                if (KeyboardCommands.Instance.DeleteObject.AreKeysDown(Keyboard))
                    DeleteObjectFromTile(tileUnderCursor.CoordsToPoint());
            }
            
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

            if (e.PressedKey == KeyboardCommands.Instance.RotateUnit.Key.Key)
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
                        if (Keyboard.IsAltHeldDown())
                            facing += 24;
                        facing = facing % 256;
                        unit.Facing = (byte)facing;
                        refreshes.Add(new RefreshPoint(unit.Position, 2));
                    }
                }

                return;
            }

            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.C && Keyboard.IsCtrlHeldDown())
            {
                copyTerrainCursorAction.StartCellCoords = null;
                CursorAction = copyTerrainCursorAction;
            }

            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.V && Keyboard.IsCtrlHeldDown())
            {
                CursorAction = pasteTerrainCursorAction;
            }
        }

        private void DrawCursorTile()
        {
            if (!IsActive)
                return;

            if (tileUnderCursor == null)
            {
                DrawString("Null tile", 0, new Vector2(0f, 40f), Color.White);
            }
            else if (CursorAction == null)
            {
                if (isDraggingObject)
                {
                    Color lineColor = Color.White;
                    if (!Map.CanMoveObject(draggedObject, tileUnderCursor.CoordsToPoint()))
                        lineColor = Color.Red;

                    Point2D cameraAndCellCenterOffset = new Point2D(-cameraTopLeftPoint.X + Constants.CellSizeX / 2,
                                                     -cameraTopLeftPoint.Y + Constants.CellSizeY / 2);

                    Point2D startDrawPoint = CellMath.CellTopLeftPoint(draggedObject.Position, Map.Size.X) + cameraAndCellCenterOffset;

                    Point2D endDrawPoint = CellMath.CellTopLeftPoint(tileUnderCursor.CoordsToPoint(), Map.Size.X) + cameraAndCellCenterOffset;

                    DrawLine(startDrawPoint.ToXNAVector(), endDrawPoint.ToXNAVector(), lineColor, 1);
                }
                else
                {
                    Point2D drawPoint = CellMath.CellTopLeftPoint(new Point2D(tileUnderCursor.X, tileUnderCursor.Y), Map.Size.X);
                    FillRectangle(new Rectangle(drawPoint.X - cameraTopLeftPoint.X + Constants.CellSizeX / 4,
                        drawPoint.Y - cameraTopLeftPoint.Y + Constants.CellSizeY / 4,
                        Constants.CellSizeX / 2, Constants.CellSizeY / 2),
                        new Color(128, 128, 128, 128));
                }
            }
        }

        public void DeleteObjectFromTile(Point2D cellCoords)
        {
            var tile = Map.GetTile(cellCoords.X, cellCoords.Y);
            if (tile == null)
                return;

            AddRefreshPoint(cellCoords);

            for (int i = 0; i < tile.Infantry.Length; i++)
            {
                if (tile.Infantry[i] != null)
                {
                    Map.RemoveInfantry(tile.Infantry[i]);
                    return;
                }
            }

            if (tile.Aircraft != null)
            {
                Map.RemoveAircraft(tile.Aircraft);
                return;
            }

            if (tile.Vehicle != null)
            {
                Map.RemoveUnit(tile.Vehicle);
                return;
            }

            if (tile.Structure != null)
            {
                Map.RemoveBuilding(tile.Structure);
                return;
            }

            if (tile.TerrainObject != null)
            {
                Map.RemoveTerrainObject(tile.CoordsToPoint());
                return;
            }

            if (tile.CellTag != null)
            {
                Map.RemoveCellTagFrom(tile.CoordsToPoint());
                return;
            }

            if (tile.Waypoint != null)
            {
                Map.RemoveWaypoint(tile.Waypoint);
                return;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (mapInvalidated)
            {
                DrawWholeMap();
                mapInvalidated = false;
            }

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PreMapDraw(tileUnderCursor.CoordsToPoint());
            }

            foreach (var refresh in refreshes)
            {
                RefreshOverArea(refresh);
            }
            refreshes.Clear();

            DrawTexture(mapRenderTarget, new Rectangle(cameraTopLeftPoint.X, cameraTopLeftPoint.Y,
                Width, Height), new Rectangle(0, 0, Width, Height), Color.White);

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PostMapDraw(tileUnderCursor.CoordsToPoint());
                CursorAction.DrawPreview(tileUnderCursor.CoordsToPoint(), cameraTopLeftPoint);
            }

            DrawCursorTile();

            base.Draw(gameTime);
        }
    }
}
