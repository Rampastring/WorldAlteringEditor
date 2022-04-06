using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Settings;
using TSMapEditor.UI;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Windows;

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
        bool OnlyPaintOnClearGround { get; }
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
        bool OnlyPaintOnClearGround { get; }
        CopiedMapData CopiedMapData { get; set; }
        Texture2D MegamapTexture { get; }
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
        public MapView(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics, EditorState editorState, MutationManager mutationManager, WindowController windowController) : base(windowManager)
        {
            EditorState = editorState;
            Map = map;
            TheaterGraphics = theaterGraphics;
            MutationManager = mutationManager;
            this.windowController = windowController;
        }

        public EditorState EditorState { get; }
        public Map Map { get; }
        public TheaterGraphics TheaterGraphics { get; }
        public MutationManager MutationManager { get; }
        private readonly WindowController windowController;

        public IMutationTarget MutationTarget => this;
        public House ObjectOwner => EditorState.ObjectOwner;
        public BrushSize BrushSize => EditorState.BrushSize;
        public Randomizer Randomizer => EditorState.Randomizer;
        public bool AutoLATEnabled => EditorState.AutoLATEnabled;
        public bool OnlyPaintOnClearGround => EditorState.OnlyPaintOnClearGround;
        public CopiedMapData CopiedMapData 
        {
            get => EditorState.CopiedMapData;
            set => EditorState.CopiedMapData = value;
        }
        public Texture2D MegamapTexture => mapRenderTarget;

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

        private int refreshSizeSetting;
        private int scrollRate;

        private bool isDraggingObject = false;
        private bool isRotatingObject = false;
        private IMovable draggedOrRotatedObject = null;

        // For right-click scrolling
        private bool isRightClickScrolling = false;
        private Point rightClickScrollInitPos = new Point(-1, -1);
        private Vector2 cameraFloatTopLeftPoint = new Vector2(-1, -1);

        /// <summary>
        /// A list of cells that have been invalidated.
        /// Areas around these cells are to be redrawn.
        /// </summary>
        //private List<RefreshPoint> refreshes = new List<RefreshPoint>();

        private List<Refresh> newRefreshes = new List<Refresh>();

        private CopyTerrainCursorAction copyTerrainCursorAction;
        private PasteTerrainCursorAction pasteTerrainCursorAction;

        private Texture2D mapWideOverlayTexture;
        private float mapWideOverlayTextureOpacity;

        private Point lastClickedPoint;

        public void AddRefreshPoint(Point2D point, int size = 1)
        {
            if (mapInvalidated)
                return;

            if (newRefreshes.Exists(nr => point == nr.InitPoint))
                return;

            var newRefresh = new Refresh(Map, refreshSizeSetting);
            newRefresh.Initiate(size, point);
            newRefreshes.Add(newRefresh);
        }

        public void InvalidateMap()
        {
            // refreshes.Clear();
            newRefreshes.Clear();
            mapInvalidated = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            const string MapWideOverlayTextureName = "mapwideoverlay.png";
            if (AssetLoader.AssetExists(MapWideOverlayTextureName))
                mapWideOverlayTexture = AssetLoader.LoadTexture(MapWideOverlayTextureName);
            EditorState.MapWideOverlayExists = mapWideOverlayTexture != null;
            mapWideOverlayTextureOpacity = UserSettings.Instance.MapWideOverlayOpacity / 255.0f;

            mapRenderTarget = CreateFullMapRenderTarget();
            objectRenderTarget = CreateFullMapRenderTarget();

            refreshSizeSetting = UserSettings.Instance.RefreshSize;
            scrollRate = UserSettings.Instance.ScrollRate;

            EditorState.CursorActionChanged += EditorState_CursorActionChanged;

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
            KeyboardCommands.Instance.FrameworkMode.Triggered += FrameworkMode_Triggered;
            KeyboardCommands.Instance.ViewMegamap.Triggered += (s, e) =>
            {
                var mmw = new MegamapWindow(WindowManager, mapRenderTarget, false);
                mmw.Width = WindowManager.RenderResolutionX;
                mmw.Height = WindowManager.RenderResolutionY;
                WindowManager.AddAndInitializeControl(mmw);
            };

            copyTerrainCursorAction = new CopyTerrainCursorAction(this);
            pasteTerrainCursorAction = new PasteTerrainCursorAction(this);

            KeyboardCommands.Instance.Copy.Triggered += (s, e) =>
            {
                copyTerrainCursorAction.StartCellCoords = null;
                copyTerrainCursorAction.EntryTypes = windowController.CopiedEntryTypesWindow.GetEnabledEntryTypes();
                CursorAction = copyTerrainCursorAction;
            };

            KeyboardCommands.Instance.Paste.Triggered += (s, e) =>
            {
                CursorAction = pasteTerrainCursorAction;
            };

            windowController.Initialized += (s, e) => windowController.MinimapWindow.MegamapClicked += MinimapWindow_MegamapClicked;
            Map.LocalSizeChanged += (s, e) => InvalidateMap();
            Map.MapResized += Map_MapResized;

            KeyboardCommands.Instance.RotateUnitOneStep.Triggered += RotateUnitOneStep_Triggered;
        }

        private void RotateUnitOneStep_Triggered(object sender, EventArgs e)
        {
            if (tileUnderCursor == null)
                return;

            var firstObject = tileUnderCursor.GetObject() as TechnoBase;
            if (firstObject == null)
                return;

            const int step = 32;

            if (firstObject.Facing + step > byte.MaxValue)
                firstObject.Facing = (byte)(firstObject.Facing + step - byte.MaxValue);
            else
                firstObject.Facing += step;

            AddRefreshPoint(tileUnderCursor.CoordsToPoint());
        }

        private void Map_MapResized(object sender, EventArgs e)
        {
            // We need to re-create our map texture
            mapRenderTarget?.Dispose();
            objectRenderTarget?.Dispose();

            mapRenderTarget = CreateFullMapRenderTarget();
            objectRenderTarget = CreateFullMapRenderTarget();

            // And then re-draw the whole map
            InvalidateMap();
        }

        private void MinimapWindow_MegamapClicked(object sender, MegamapClickedEventArgs e)
        {
            cameraTopLeftPoint = e.ClickedPoint - new Point2D(Width / 2, Height / 2);
            ConstrainCamera();
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
                    Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(i, j), Map.Size.X);
                    if (row[j] == null)
                    {
                        DrawString("n", 0, drawPoint.ToXNAVector() + new Vector2(Constants.CellSizeX / 2, Constants.CellSizeY / 2), Color.Red, 0.5f);
                        continue;
                    }

                    DrawTerrainTile(row[j]);
                }
            }

            DrawSmudges();
            DrawOverlays();

            DrawObjects();
            DrawCellTags();
            DrawWaypoints();
            DrawMapBorder();

            DrawTubes();

            Renderer.PopRenderTarget();

            sw.Stop();
            Console.WriteLine("Map render time: " + sw.Elapsed.TotalMilliseconds);
        }

        private void DrawSmudges()
        {
            Map.DoForAllValidTiles(t =>
            {
                if (t.Smudge != null)
                    DrawObject(t.Smudge);
            });
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
                if (t.Smudge != null)
                    DrawObject(t.Smudge); 
            });
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
            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, Map.Size.X);

            ObjectImage graphics = null;
            Color replacementColor = Color.Red;
            Color remapColor = Color.White;
            Color foundationLineColor = Color.White;
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
                    foundationLineColor = structure.Owner.XNAColor;
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
                case RTTIType.Smudge:
                    var smudge = (Smudge)gameObject;
                    if (smudge.SmudgeType == null)
                        return;
                    graphics = TheaterGraphics.SmudgeTextures[smudge.SmudgeType.Index];
                    replacementColor = Color.Cyan;
                    iniName = smudge.SmudgeType.ININame;
                    break;
            }

            if (gameObject.WhatAmI() == RTTIType.Building)
            {
                var structure = (Structure)gameObject;
                int foundationX = structure.ObjectType.ArtConfig.FoundationX;
                int foundationY = structure.ObjectType.ArtConfig.FoundationY;
                if (foundationX > 0 && foundationY > 0)
                {
                    Point2D p1 = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p2 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p3 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X, gameObject.Position.Y + foundationY), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p4 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y + foundationY), Map.Size.X) + new Point2D(Constants.CellSizeX / 2, 0);

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
            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(tile.X, tile.Y), Map.Size.X);
            
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

            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(waypoint.Position, Map.Size.X);

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

            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(cellTag.Position, Map.Size.X);

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
            int y = (int)(((double)Map.LocalSize.Y - 2.5) * Constants.CellSizeY);
            int width = Map.LocalSize.Width * Constants.CellSizeX;
            int height = (Map.LocalSize.Height + 4) * Constants.CellSizeY;

            DrawRectangle(new Rectangle(x, y, width, height), Color.Blue, 4);
        }

        public override void OnMouseOnControl()
        {
            if (CursorAction == null && (isDraggingObject || isRotatingObject))
            {
                if (!Cursor.LeftDown)
                {
                    if (isDraggingObject)
                    {
                        isDraggingObject = false;

                        if (tileUnderCursor != null && tileUnderCursor.CoordsToPoint() != draggedOrRotatedObject.Position)
                        {
                            if (Map.CanMoveObject(draggedOrRotatedObject, tileUnderCursor.CoordsToPoint()))
                            {
                                var mutation = new MoveObjectMutation(MutationTarget, draggedOrRotatedObject, tileUnderCursor.CoordsToPoint());
                                MutationManager.PerformMutation(mutation);
                            }
                        }
                    }
                    else if (isRotatingObject)
                    {
                        isRotatingObject = false;
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

                    ConstrainCamera();
                }
            }

            base.OnMouseOnControl();
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

            if (CursorAction == null && tileUnderCursor != null && Cursor.LeftDown && !isDraggingObject && !isRotatingObject)
            {
                var cellObject = tileUnderCursor.GetObject();

                if (cellObject != null)
                {
                    draggedOrRotatedObject = tileUnderCursor.GetObject();

                    if (draggedOrRotatedObject != null)
                    {
                        if (KeyboardCommands.Instance.RotateUnit.AreKeysDown(Keyboard))
                            isRotatingObject = true;
                        else
                            isDraggingObject = true;
                    }
                }
                else if (tileUnderCursor.Waypoint != null)
                {
                    draggedOrRotatedObject = tileUnderCursor.Waypoint;
                    isDraggingObject = true;
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
            else
            {
                var cursorPoint = GetCursorPoint();
                if (cursorPoint == lastClickedPoint)
                {
                    HandleDoubleClick();
                }
                else
                {
                    lastClickedPoint = cursorPoint;
                }
            }

            base.OnLeftClick();
        }

        private void HandleDoubleClick()
        {
            if (tileUnderCursor != null && CursorAction == null)
            {
                if (tileUnderCursor.Structure != null)
                    windowController.StructureOptionsWindow.Open(tileUnderCursor.Structure);

                if (tileUnderCursor.Vehicle != null)
                    windowController.VehicleOptionsWindow.Open(tileUnderCursor.Vehicle);

                Infantry infantry = tileUnderCursor.GetFirstInfantry();
                if (infantry != null)
                    windowController.InfantryOptionsWindow.Open(infantry);
            }
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

                ConstrainCamera();
            }

            windowController.MinimapWindow.CameraRectangle = new Rectangle(cameraTopLeftPoint.ToXNAPoint(), new Point(Width, Height));

            Point2D cursorMapPoint = GetCursorMapPoint();
            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(cursorMapPoint, Map.Size);
            var tile = Map.GetTile(tileCoords.X, tileCoords.Y);
            tileUnderCursor = tile;
            TileInfoDisplay.MapTile = tile;

            if (IsActive && tileUnderCursor != null)
            {
                if (KeyboardCommands.Instance.DeleteObject.AreKeysDown(Keyboard))
                    DeleteObjectFromCell(tileUnderCursor.CoordsToPoint());
            }
            
            base.Update(gameTime);
        }

        private void ConstrainCamera()
        {
            int minX = -WindowManager.RenderResolutionX / 2;
            if (cameraTopLeftPoint.X < minX)
                cameraTopLeftPoint = new Point2D(minX, cameraTopLeftPoint.Y);

            if (cameraFloatTopLeftPoint.X < minX)
                cameraFloatTopLeftPoint = new Vector2(minX, cameraFloatTopLeftPoint.Y);

            int minY = -WindowManager.RenderResolutionY / 2;
            if (cameraTopLeftPoint.Y < minY)
                cameraTopLeftPoint = new Point2D(cameraTopLeftPoint.X, minY);

            if (cameraFloatTopLeftPoint.Y < minY)
                cameraFloatTopLeftPoint = new Vector2(cameraFloatTopLeftPoint.X, minY);

            int maxX = Map.Size.X * Constants.CellSizeX - WindowManager.RenderResolutionX / 2;
            if (cameraTopLeftPoint.X > maxX)
                cameraTopLeftPoint = new Point2D(maxX, cameraTopLeftPoint.Y);

            if (cameraFloatTopLeftPoint.X > maxX)
                cameraFloatTopLeftPoint = new Vector2(maxX, cameraFloatTopLeftPoint.Y);

            int maxY = Map.Size.Y * Constants.CellSizeY - WindowManager.RenderResolutionY / 2;
            if (cameraTopLeftPoint.Y > maxY)
                cameraTopLeftPoint = new Point2D(cameraTopLeftPoint.X, maxY);

            if (cameraFloatTopLeftPoint.Y > maxY)
                cameraFloatTopLeftPoint = new Vector2(cameraFloatTopLeftPoint.X, maxY);
        }

        private Point2D GetCursorMapPoint()
        {
            Point cursorPoint = GetCursorPoint();
            Point2D cursorMapPoint = new Point2D(cameraTopLeftPoint.X + cursorPoint.X,
                    cameraTopLeftPoint.Y + cursorPoint.Y);
            return cursorMapPoint;
        }

        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            if (!IsActive)
                return;

            if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.F1)
            {
                var text = new StringBuilder();

                foreach (KeyboardCommand command in KeyboardCommands.Instance.Commands)
                {
                    text.Append(command.UIName + ": " + command.GetKeyDisplayString());
                    text.Append(Environment.NewLine);
                }

                EditorMessageBox.Show(WindowManager, "Hotkey Help", text.ToString(), MessageBoxButtons.OK);
            }

            if (!e.Handled && CursorAction != null && CursorAction.HandlesKeyboardInput)
            {
                CursorAction.OnKeyPressed(e);
            }
        }

        private void DrawOnTileUnderCursor()
        {
            if (!IsActive)
                return;

            if (tileUnderCursor == null)
            {
                DrawString("Null tile", 0, new Vector2(0f, 40f), Color.White);
                return;
            }

            if (CursorAction != null)
            {
                if (CursorAction.DrawCellCursor)
                    DrawTileCursor();

                return;
            }


            if (isDraggingObject)
            {
                Color lineColor = Color.White;
                if (!Map.CanMoveObject(draggedOrRotatedObject, tileUnderCursor.CoordsToPoint()))
                    lineColor = Color.Red;

                Point2D cameraAndCellCenterOffset = new Point2D(-cameraTopLeftPoint.X + Constants.CellSizeX / 2,
                                                 -cameraTopLeftPoint.Y + Constants.CellSizeY / 2);

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map.Size.X) + cameraAndCellCenterOffset;

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map.Size.X) + cameraAndCellCenterOffset;

                DrawLine(startDrawPoint.ToXNAVector(), endDrawPoint.ToXNAVector(), lineColor, 1);
            }
            else if (isRotatingObject)
            {
                Color lineColor = Color.Yellow;

                Point2D cameraAndCellCenterOffset = new Point2D(-cameraTopLeftPoint.X + Constants.CellSizeX / 2,
                                                 -cameraTopLeftPoint.Y + Constants.CellSizeY / 2);

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map.Size.X) + cameraAndCellCenterOffset;

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map.Size.X) + cameraAndCellCenterOffset;

                DrawLine(startDrawPoint.ToXNAVector(), endDrawPoint.ToXNAVector(), lineColor, 1);

                if (draggedOrRotatedObject.IsTechno())
                {
                    var techno = (TechnoBase)draggedOrRotatedObject;
                    Point2D point = tileUnderCursor.CoordsToPoint() - draggedOrRotatedObject.Position;

                    float angle = point.Angle() + ((float)Math.PI / 2.0f);
                    if (angle > (float)Math.PI * 2.0f)
                    {
                        angle = angle - ((float)Math.PI * 2.0f);
                    }
                    else if (angle < 0f)
                    {
                        angle += (float)Math.PI * 2.0f;
                    }

                    float percent = angle / ((float)Math.PI * 2.0f);
                    byte facing = (byte)Math.Ceiling(percent * (float)byte.MaxValue);

                    techno.Facing = facing;
                    AddRefreshPoint(techno.Position, 2);
                }
            }
            else
            {
                DrawTileCursor();
            }
        }

        private void DrawTileCursor()
        {
            Color lineColor = new Color(96, 168, 96, 128);
            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(tileUnderCursor.X, tileUnderCursor.Y), Map.Size.X) - cameraTopLeftPoint;

            var cellTopPoint = new Vector2(cellTopLeftPoint.X + Constants.CellSizeX / 2, cellTopLeftPoint.Y);
            var cellLeftPoint = new Vector2(cellTopLeftPoint.X, cellTopLeftPoint.Y + Constants.CellSizeY / 2);
            var cellRightPoint = new Vector2(cellTopLeftPoint.X + Constants.CellSizeX, cellLeftPoint.Y);
            var cellBottomPoint = new Vector2(cellTopPoint.X, cellTopLeftPoint.Y + Constants.CellSizeY);

            DrawLine(cellTopPoint, cellLeftPoint, lineColor, 1);
            DrawLine(cellRightPoint, cellTopPoint, lineColor, 1);
            DrawLine(cellBottomPoint, cellLeftPoint, lineColor, 1);
            DrawLine(cellRightPoint, cellBottomPoint, lineColor, 1);

            var shadowColor = new Color(0, 0, 0, 128);
            var down = new Vector2(0, 1f);

            DrawLine(cellTopPoint + down, cellLeftPoint + down, shadowColor, 1);
            DrawLine(cellRightPoint + down, cellTopPoint + down, shadowColor, 1);
            DrawLine(cellBottomPoint + down, cellLeftPoint + down, shadowColor, 1);
            DrawLine(cellRightPoint + down, cellBottomPoint + down, shadowColor, 1);
        }

        public void DeleteObjectFromCell(Point2D cellCoords)
        {
            var tile = Map.GetTile(cellCoords.X, cellCoords.Y);
            if (tile == null)
                return;

            AddRefreshPoint(cellCoords, 2);
            Map.DeleteObjectFromCell(cellCoords);
        }

        private void DrawTubes()
        {
            foreach (var tube in Map.Tubes)
            {
                var entryCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.EntryPoint, Map.Size.X);
                var exitCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.ExitPoint, Map.Size.X);

                Point2D currentPoint = tube.EntryPoint;

                foreach (var direction in tube.Directions)
                {
                    Point2D nextPoint = currentPoint.NextPointFromTubeDirection(direction);

                    if (nextPoint != currentPoint)
                    {
                        var currentPixelPoint = CellMath.CellCenterPointFromCellCoords(currentPoint, Map.Size.X);
                        var nextPixelPoint = CellMath.CellCenterPointFromCellCoords(nextPoint, Map.Size.X);

                        DrawArrow(currentPixelPoint.ToXNAVector(), nextPixelPoint.ToXNAVector(), Color.LimeGreen, 0.25f, 10f, 2);
                    }

                    currentPoint = nextPoint;
                }
            }
        }

        private static void DrawArrow(Vector2 start, Vector2 end,
            Color color, float angleDiff, float sideLineLength, int thickness = 1)
        {
            Vector2 line = end - start;
            float angle = Helpers.AngleFromVector(line) - (float)Math.PI;
            Renderer.DrawLine(start,
                end, color, thickness);
            Renderer.DrawLine(end, end + Helpers.VectorFromLengthAndAngle(sideLineLength, angle + angleDiff),
                color, thickness);
            Renderer.DrawLine(end, end + Helpers.VectorFromLengthAndAngle(sideLineLength, angle - angleDiff),
                color, thickness);
        }

        public override void Draw(GameTime gameTime)
        {
            if (mapInvalidated)
            {
                DrawWholeMap();
                mapInvalidated = false;
                newRefreshes.Clear();
                DrawTubes();
            }

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PreMapDraw(tileUnderCursor.CoordsToPoint());
            }

            if (newRefreshes.Count > 0)
            {
                Renderer.PushRenderTarget(mapRenderTarget);

                int i = 0;
                while (i < newRefreshes.Count)
                {
                    var refresh = newRefreshes[i];

                    if (!refresh.IsComplete)
                    {
                        refresh.Process();

                        if (!refresh.IsComplete)
                        {
                            i++;
                            continue;
                        }
                    }

                    var overlaysToRedraw = new List<Overlay>();
                    var smudgesToRedraw = new List<Smudge>();
                    var waypointsToRedraw = new List<Waypoint>();
                    var cellTagsToRedraw = new List<CellTag>();

                    var sortedCells = refresh.tilesToRedraw.Select(kvp => kvp.Value).OrderBy(cell => cell.Y).ThenBy(cell => cell.X).ToArray();

                    foreach (var cell in sortedCells)
                    {
                        DrawTerrainTile(cell);

                        if (cell.Overlay != null)
                            overlaysToRedraw.Add(cell.Overlay);
                        if (cell.Smudge != null)
                            smudgesToRedraw.Add(cell.Smudge);
                        if (cell.Waypoint != null)
                            waypointsToRedraw.Add(cell.Waypoint);
                        if (cell.CellTag != null)
                            cellTagsToRedraw.Add(cell.CellTag);
                    }

                    overlaysToRedraw.ForEach(o => DrawObject(o));
                    smudgesToRedraw.ForEach(o => DrawObject(o));
                    var sortedObjects = refresh.objectsToRedraw.Select(kvp => kvp.Value).OrderBy(go => go.GetYPositionForDrawOrder()).ThenBy(go => go.GetXPositionForDrawOrder()).ThenBy(go => go.WhatAmI()).ToArray();
                    Array.ForEach(sortedObjects, obj => DrawObject(obj));
                    waypointsToRedraw.ForEach(wp => DrawWaypoint(wp));
                    cellTagsToRedraw.ForEach(ct => DrawCellTag(ct));

                    newRefreshes.RemoveAt(i);
                }

                DrawTubes();
                DrawMapBorder();
                Renderer.PopRenderTarget();
            }


            // foreach (var refresh in refreshes)
            // {
            //     RefreshOverArea(refresh);
            // }
            // refreshes.Clear();

            DrawTexture(mapRenderTarget, new Rectangle(cameraTopLeftPoint.X, cameraTopLeftPoint.Y,
                Width, Height), new Rectangle(0, 0, Width, Height), Color.White);

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PostMapDraw(tileUnderCursor.CoordsToPoint());
                CursorAction.DrawPreview(tileUnderCursor.CoordsToPoint(), cameraTopLeftPoint);
            }

            if (mapWideOverlayTexture != null && EditorState.DrawMapWideOverlay)
            {
                Renderer.DrawTexture(mapWideOverlayTexture,
                    new Rectangle(-cameraTopLeftPoint.X, -cameraTopLeftPoint.Y, mapRenderTarget.Width, mapRenderTarget.Height),
                    Color.White * mapWideOverlayTextureOpacity);
            }

            DrawOnTileUnderCursor();

            base.Draw(gameTime);
        }
    }
}
