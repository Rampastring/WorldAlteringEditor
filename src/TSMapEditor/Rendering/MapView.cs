using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using TSMapEditor.CCEngine;
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
#if !ADVMAPVIEW
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
        Camera Camera { get; }
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
#endif

#if !ADVMAPVIEW
    public class MapView : XNAControl, ICursorActionTarget, IMutationTarget
#else
    public class OldMapView : XNAControl, ICursorActionTarget, IMutationTarget
#endif
    {
        private const float RightClickScrollRateDivisor = 64f;

#if !ADVMAPVIEW
        public MapView(
#else
        public OldMapView(
#endif
        WindowManager windowManager, Map map, TheaterGraphics theaterGraphics, EditorState editorState, MutationManager mutationManager, WindowController windowController) : base(windowManager)
        {
            EditorState = editorState;
            Map = map;
            TheaterGraphics = theaterGraphics;
            MutationManager = mutationManager;
            this.windowController = windowController;

            Camera = new Camera(WindowManager, Map);
            Camera.CameraUpdated += (s, e) => cameraMoved = true;
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
        public Texture2D MegamapTexture => compositeRenderTarget;
        public Camera Camera { get; private set; }

        public TileInfoDisplay TileInfoDisplay { get; set; }
        
        public CursorAction CursorAction
        {
            get => EditorState.CursorAction;
            set => EditorState.CursorAction = value;
        }

        private Texture2D impassableCellHighlightTexture;
        private Texture2D iceGrowthHighlightTexture;

        private RenderTarget2D mapRenderTarget;
        private RenderTarget2D depthRenderTarget;
        private RenderTarget2D depthRenderTargetCopy;
        private RenderTarget2D objectRenderTarget;
        private RenderTarget2D shadowRenderTarget;
        private RenderTarget2D transparencyRenderTarget;
        private RenderTarget2D compositeRenderTarget;

        private Effect colorDrawEffect;
        private Effect depthApplyEffect;

        private MapTile tileUnderCursor;
        private MapTile lastTileUnderCursor;

        private bool mapInvalidated;
        private bool cameraMoved;

        private int scrollRate;

        private bool isDraggingObject = false;
        private bool isRotatingObject = false;
        private IMovable draggedOrRotatedObject = null;

        private bool isRightClickScrolling = false;
        private Point rightClickScrollInitPos = new Point(-1, -1);

        private MapWideOverlay mapWideOverlay;

        private Point lastClickedPoint;

        private List<GameObject> gameObjectsToRender = new List<GameObject>();
        private List<Smudge> smudgesToRender = new List<Smudge>();
        private List<Overlay> overlaysToRender = new List<Overlay>();

        private Stopwatch refreshStopwatch;

        private ulong refreshIndex;

        private bool isRenderingDepth;

        private bool debugRenderDepthBuffer = false;


        public void AddRefreshPoint(Point2D point, int size = 1)
        {
            InvalidateMap();
        }

        public void InvalidateMap()
        {
            if (!mapInvalidated)
                refreshIndex++;

            mapInvalidated = true;
        }

        public override void Initialize()
        {
            base.Initialize();

            LoadShaders();

            impassableCellHighlightTexture = AssetLoader.LoadTexture("impassablehighlight.png");
            iceGrowthHighlightTexture = AssetLoader.LoadTexture("icehighlight.png");

            mapWideOverlay = new MapWideOverlay();
            EditorState.MapWideOverlayExists = mapWideOverlay.HasTexture;

            RefreshRenderTargets();

            scrollRate = UserSettings.Instance.ScrollRate;

            EditorState.CursorActionChanged += EditorState_CursorActionChanged;

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;
            KeyboardCommands.Instance.FrameworkMode.Triggered += FrameworkMode_Triggered;
            KeyboardCommands.Instance.ViewMegamap.Triggered += (s, e) =>
            {
                var mmw = new MegamapWindow(WindowManager, compositeRenderTarget, false);
                mmw.Width = WindowManager.RenderResolutionX;
                mmw.Height = WindowManager.RenderResolutionY;
                WindowManager.AddAndInitializeControl(mmw);
            };

            windowController.Initialized += (s, e) => windowController.MinimapWindow.MegamapClicked += MinimapWindow_MegamapClicked;
            Map.LocalSizeChanged += (s, e) => InvalidateMap();
            Map.MapResized += Map_MapResized;

            Map.HouseColorChanged += (s, e) => InvalidateMap();
            EditorState.HighlightImpassableCellsChanged += (s, e) => InvalidateMap();
            EditorState.HighlightIceGrowthChanged += (s, e) => InvalidateMap();
            EditorState.DrawMapWideOverlayChanged += (s, e) => mapWideOverlay.Enabled = EditorState.DrawMapWideOverlay;

            KeyboardCommands.Instance.RotateUnitOneStep.Triggered += RotateUnitOneStep_Triggered;

            refreshStopwatch = new Stopwatch();

            Keyboard.OnKeyPressed += (s, e) =>
            {
                if (e.PressedKey == Microsoft.Xna.Framework.Input.Keys.F11)
                    debugRenderDepthBuffer = !debugRenderDepthBuffer;
            };

            InvalidateMap();
        }

        private void LoadShaders()
        {
            colorDrawEffect = AssetLoader.LoadEffect("Shaders/ColorDraw");
            depthApplyEffect = AssetLoader.LoadEffect("Shaders/DepthApply");
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
            // We need to re-create our map textures
            RefreshRenderTargets();

            windowController.MinimapWindow.MegamapTexture = mapRenderTarget;

            // And then re-draw the whole map
            InvalidateMap();
        }

        private void RefreshRenderTargets()
        {
            mapRenderTarget?.Dispose();
            depthRenderTarget?.Dispose();
            depthRenderTargetCopy?.Dispose();
            objectRenderTarget?.Dispose();
            shadowRenderTarget?.Dispose();
            transparencyRenderTarget?.Dispose();
            compositeRenderTarget?.Dispose();

            mapRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Color, DepthFormat.Depth16);
            depthRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Single);
            depthRenderTargetCopy = CreateFullMapRenderTarget(SurfaceFormat.Single);
            objectRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Color);
            shadowRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Alpha8);
            transparencyRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Color);
            compositeRenderTarget = CreateFullMapRenderTarget(SurfaceFormat.Color);
        }

        private void MinimapWindow_MegamapClicked(object sender, MegamapClickedEventArgs e)
        {
            Camera.TopLeftPoint = e.ClickedPoint - new Point2D(Width / 2, Height / 2).ScaleBy(1.0 / Camera.ZoomLevel);
        }

        private void FrameworkMode_Triggered(object sender, EventArgs e)
        {
            EditorState.IsMarbleMadness = !EditorState.IsMarbleMadness;
            InvalidateMap();
        }

        private void EditorState_CursorActionChanged(object sender, EventArgs e)
        {
            if (lastTileUnderCursor != null)
                AddRefreshPoint(lastTileUnderCursor.CoordsToPoint(), 3);

            lastTileUnderCursor = null;
        }

        private RenderTarget2D CreateFullMapRenderTarget(SurfaceFormat surfaceFormat, DepthFormat depthFormat = DepthFormat.None)
        {
           return new RenderTarget2D(GraphicsDevice,
               Map.WidthInPixels,
               Map.HeightInPixels, false, surfaceFormat,
               depthFormat, 0, RenderTargetUsage.PreserveContents);
        }

        public void DrawVisibleMapPortion()
        {
            refreshStopwatch.Restart();

            smudgesToRender.Clear();
            overlaysToRender.Clear();
            gameObjectsToRender.Clear();

            if (mapInvalidated)
            {
                GraphicsDevice.SetRenderTarget(depthRenderTarget);
                GraphicsDevice.Clear(Color.White);
                GraphicsDevice.SetRenderTarget(depthRenderTargetCopy);
                GraphicsDevice.Clear(Color.White);
            }

            // First render depth
            Renderer.PushRenderTarget(depthRenderTarget);
            isRenderingDepth = true;
            Renderer.PushSettings(new SpriteBatchSettings(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, depthApplyEffect));
            DoForVisibleCells(DrawTerrainTile);
            Renderer.PopSettings();
            isRenderingDepth = false;

            Renderer.PopRenderTarget();

            // Copy rendered depth so we are able to sample the depth buffer when rendering depth on later frames
            Renderer.PushRenderTarget(depthRenderTargetCopy, new SpriteBatchSettings(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, null));
            Renderer.DrawTexture(depthRenderTarget, new Rectangle(0, 0, depthRenderTargetCopy.Width, depthRenderTargetCopy.Height), Color.White);
            Renderer.PopRenderTarget();

            // Then render color
            Renderer.PushRenderTarget(mapRenderTarget);
            // GraphicsDevice.Clear(Color.Black);

            var colorDrawSettings = new SpriteBatchSettings(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, colorDrawEffect);
            Renderer.PushSettings(colorDrawSettings);
            DoForVisibleCells(DrawTerrainTileAndRegisterObjects);

            Renderer.PushRenderTarget(objectRenderTarget, colorDrawSettings);
            GraphicsDevice.Clear(Color.Transparent);
            DrawSmudges();
            DrawOverlays();
            DrawObjects();
            Renderer.PopRenderTarget();

            Renderer.PopSettings();

            // Then draw on-map UI elements
            DrawMapUIElements();

            Renderer.PopRenderTarget();

            refreshStopwatch.Stop();
            Console.WriteLine("Map render time: " + refreshStopwatch.Elapsed.TotalMilliseconds);
        }

        private void DrawMapUIElements()
        {
            Renderer.PushRenderTarget(transparencyRenderTarget, new SpriteBatchSettings(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null));
            GraphicsDevice.Clear(Color.Transparent);

            Map.GraphicalBaseNodes.ForEach(DrawBaseNode);

            DrawCellTags();
            DrawWaypoints();

            DrawTubes();

            if (EditorState.HighlightImpassableCells)
            {
                Map.DoForAllValidTiles(DrawImpassableHighlight);
            }

            if (EditorState.HighlightIceGrowth)
            {
                Map.DoForAllValidTiles(DrawIceGrowthHighlight);
            }

            Renderer.PopRenderTarget();
        }

        private void SetEffectParams(Effect effect, float bottomDepth, float topDepth,
            Vector2 worldTextureCoordinates, Vector2 spriteSizeToWorldSizeRatio, Texture2D depthTexture)
        {
            effect.Parameters["SpriteDepthBottom"].SetValue(bottomDepth);
            effect.Parameters["SpriteDepthTop"].SetValue(topDepth);
            effect.Parameters["WorldTextureCoordinates"].SetValue(worldTextureCoordinates);
            effect.Parameters["SpriteSizeToWorldSizeRatio"].SetValue(spriteSizeToWorldSizeRatio);
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            if (depthTexture != null)
            {
                effect.Parameters["DepthTexture"].SetValue(depthTexture);
                GraphicsDevice.Textures[1] = depthTexture;
            }
        }

        private void DoForVisibleCells(Action<MapTile> action)
        {
            // Add some padding to take objects just outside of the visible screen to account
            int tlX = Camera.TopLeftPoint.X - Constants.RenderPixelPadding;
            int tlY = Camera.TopLeftPoint.Y - Constants.RenderPixelPadding;

            if (tlX < 0)
                tlX = 0;

            if (tlY < 0)
                tlY = 0;

            Point2D firstVisibleCellCoords = CellMath.CellCoordsFromPixelCoords_2D(new Point2D(tlX, tlY), Map);

            int camRight = GetCameraRightXCoord() + Constants.RenderPixelPadding;
            int camBottom = GetCameraBottomYCoord() + Constants.RenderPixelPadding;

            int xCellCount = (camRight - tlX) / Constants.CellSizeX;
            xCellCount += 2; // Add some padding for edge cases

            int yCellCount = (camBottom - tlY) / Constants.CellSizeY;

            // Add some padding to take height levels into account
            const int yPadding = 8;
            yCellCount += yPadding;

            for (int offset = 0; offset < yCellCount; offset++)
            {
                int x = firstVisibleCellCoords.X + offset;
                int y = firstVisibleCellCoords.Y + offset;

                // Draw two horizontal rows of the map

                for (int sx = 0; sx < xCellCount; sx++)
                {
                    int coordX = x + sx;
                    int coordY = y - sx;

                    var cell = Map.GetTile(coordX, coordY);

                    if (cell != null)
                        action(cell);
                }

                for (int sx = 0; sx < xCellCount; sx++)
                {
                    int coordX = x + 1 + sx;
                    int coordY = y - sx;

                    var cell = Map.GetTile(coordX, coordY);

                    if (cell != null)
                        action(cell);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCameraWidth() => (int)(Width / (float)Camera.ZoomLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCameraHeight() => (int)(Height / (float)Camera.ZoomLevel);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCameraRightXCoord() => Math.Min(Camera.TopLeftPoint.X + GetCameraWidth(), Map.Size.X * Constants.CellSizeX);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetCameraBottomYCoord() => Math.Min(Camera.TopLeftPoint.Y + GetCameraHeight(), Map.Size.Y * Constants.CellSizeY);

        public void DrawTerrainTileAndRegisterObjects(MapTile tile)
        {
            DrawTerrainTile(tile);

            if (tile.Smudge != null)
                smudgesToRender.Add(tile.Smudge);

            if (tile.Overlay != null && tile.Overlay.OverlayType != null)
                overlaysToRender.Add(tile.Overlay);

            if (tile.Structure != null && tile.Structure.Position == tile.CoordsToPoint())
                gameObjectsToRender.Add(tile.Structure);

            tile.DoForAllInfantry(i => gameObjectsToRender.Add(i));

            if (tile.Aircraft != null)
                gameObjectsToRender.Add(tile.Aircraft);

            if (tile.Vehicle != null)
                gameObjectsToRender.Add(tile.Vehicle);

            if (tile.TerrainObject != null)
                gameObjectsToRender.Add(tile.TerrainObject);
        }

        public void DrawTerrainTile(MapTile tile)
        {
            if (tile.LastRefreshIndex == refreshIndex)
                return;

            if (tile.TileIndex >= TheaterGraphics.TileCount)
                return;

            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(tile.X, tile.Y), Map);

            if (drawPoint.X + Constants.CellSizeX < Camera.TopLeftPoint.X || drawPoint.X > GetCameraRightXCoord())
                return;

            if (drawPoint.Y + Constants.CellSizeY < Camera.TopLeftPoint.Y)
                return;

            if (tile.TileImage == null)
                tile.TileImage = TheaterGraphics.GetTileGraphics(tile.TileIndex);

            TileImage tileImage;
            int subTileIndex;
            int level;
            if (tile.PreviewTileImage != null)
            {
                tileImage = tile.PreviewTileImage;
                subTileIndex = tile.PreviewSubTileIndex;
                level = tile.PreviewLevel;
            }
            else
            {
                tileImage = tile.TileImage;
                subTileIndex = tile.SubTileIndex;
                level = tile.Level;
            }

            // Framework Mode / Marble Madness support
            if (EditorState.IsMarbleMadness)
                tileImage = TheaterGraphics.GetMarbleMadnessTileGraphics(tileImage.TileID);

            if (subTileIndex >= tileImage.TMPImages.Length)
                subTileIndex = 0;

            if (tileImage.TMPImages.Length == 0)
                return;

            MGTMPImage tmpImage = tileImage.TMPImages[subTileIndex];

            if (tmpImage.TmpImage == null)
                return;

            int extraDrawY = drawPoint.Y + tmpImage.TmpImage.YExtra - tmpImage.TmpImage.Y;

            if (drawPoint.Y - (tile.Level * Constants.CellHeight) > GetCameraBottomYCoord())
            {
                if (tmpImage.ExtraTexture == null)
                    return;

                // If we have extra graphics, need to check whether they are on screen
                if (extraDrawY - (tile.Level * Constants.CellHeight) > GetCameraBottomYCoord())
                    return;
            }

            // We know we're going to render this tile - update refresh index
            if (!isRenderingDepth)
                tile.LastRefreshIndex = refreshIndex;

            int originalDrawPointY = drawPoint.Y;
            drawPoint -= new Point2D(0, (Constants.CellSizeY / 2) * level);

            if (subTileIndex >= tileImage.TMPImages.Length)
            {
                DrawString(subTileIndex.ToString(), 0, new Vector2(drawPoint.X, drawPoint.Y), Color.Red);
                return;
            }

            if (tmpImage.Texture != null)
            {
                float depthTop = originalDrawPointY / (float)Map.HeightInPixels;
                float depthBottom = (originalDrawPointY + tmpImage.Texture.Height) / (float)Map.HeightInPixels;
                depthTop = 1.0f - depthTop;
                depthBottom = 1.0f - depthBottom;

                Vector2 worldTextureCoordinates = new Vector2(drawPoint.X / (float)Map.WidthInPixels, drawPoint.Y / (float)Map.HeightInPixels);
                Vector2 spriteSizeToWorldSizeRatio = new Vector2(Constants.CellSizeX / (float)Map.WidthInPixels, Constants.CellSizeY / (float)Map.HeightInPixels);

                if (isRenderingDepth)
                    SetEffectParams(depthApplyEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTargetCopy);
                else
                    SetEffectParams(colorDrawEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTarget);

                DrawTexture(tmpImage.Texture, new Rectangle(drawPoint.X, drawPoint.Y,
                    Constants.CellSizeX, Constants.CellSizeY), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }

            if (tmpImage.ExtraTexture != null)
            {
                int yDrawPointWithoutCellHeight = originalDrawPointY + tmpImage.TmpImage.YExtra - tmpImage.TmpImage.Y;
                float depthTop = yDrawPointWithoutCellHeight / (float)Map.HeightInPixels;
                float depthBottom = (yDrawPointWithoutCellHeight + tmpImage.ExtraTexture.Height) / (float)Map.HeightInPixels;
                depthTop = depthTop - ((depthTop - depthBottom) / 4.0f);
                depthTop = 1.0f - depthTop;
                depthBottom = 1.0f - depthBottom;
                // depthTop = depthBottom; // depthTop - ((depthTop - depthBottom) / 2.0f);
                // depthBottom = Math.Max(0f, depthTop * 0.9f);

                int exDrawPointX = drawPoint.X + tmpImage.TmpImage.XExtra - tmpImage.TmpImage.X;
                int exDrawPointY = drawPoint.Y + tmpImage.TmpImage.YExtra - tmpImage.TmpImage.Y;

                Vector2 worldTextureCoordinates = new Vector2(exDrawPointX / (float)Map.WidthInPixels, exDrawPointY / (float)Map.HeightInPixels);
                Vector2 spriteSizeToWorldSizeRatio = new Vector2(tmpImage.ExtraTexture.Width / (float)Map.WidthInPixels, tmpImage.ExtraTexture.Height / (float)Map.HeightInPixels);

                if (isRenderingDepth)
                    SetEffectParams(depthApplyEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTargetCopy);
                else
                    SetEffectParams(colorDrawEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTarget);

                DrawTexture(tmpImage.ExtraTexture,
                    new Rectangle(exDrawPointX,
                    exDrawPointY,
                    tmpImage.ExtraTexture.Width,
                    tmpImage.ExtraTexture.Height),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero, SpriteEffects.None, 0f);
            }
        }

        private void DrawWaypoints()
        {
            Map.Waypoints.ForEach(DrawWaypoint);
        }

        private void DrawCellTags()
        {
            DoForVisibleCells(t =>
            {
                if (t.CellTag != null)
                    DrawCellTag(t.CellTag);
            });
        }

        private int CompareGameObjectsForRendering(GameObject obj1, GameObject obj2)
        {
            return GenericObjectRenderingComparison(obj1, obj2);
        }

        private int GenericObjectRenderingComparison(GameObject obj1, GameObject obj2)
        {
            int val1 = obj1.GetYPositionForDrawOrder();
            int val2 = obj2.GetYPositionForDrawOrder();

            if (val1 > val2)
                return 1;
            else if (val1 < val2)
                return -1;

            int xResult = obj1.GetXPositionForDrawOrder().CompareTo(obj2.GetXPositionForDrawOrder());
            if (xResult != 0)
                return xResult;

            return ((int)obj1.WhatAmI()).CompareTo((int)obj2.WhatAmI());
        }

        private void DrawSmudges()
        {
            smudgesToRender.Sort(CompareGameObjectsForRendering);
            smudgesToRender.ForEach(DrawObject);
        }

        private void DrawOverlays()
        {
            overlaysToRender.Sort(CompareGameObjectsForRendering);
            overlaysToRender.ForEach(DrawObject);
        }

        private void DrawObjects()
        {
            // gameObjectsToRender.Clear();
            // gameObjectsToRender.AddRange(Map.TerrainObjects);
            // gameObjectsToRender.AddRange(Map.Structures);
            // gameObjectsToRender.AddRange(Map.Aircraft);
            // gameObjectsToRender.AddRange(Map.Units);
            // gameObjectsToRender.AddRange(Map.Infantry);
            gameObjectsToRender.Sort(CompareGameObjectsForRendering);
            gameObjectsToRender.ForEach(DrawObject);
        }

        private void DrawObject(GameObject gameObject)
        {
            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, Map);

            ObjectImage graphics = null;
            Color replacementColor = Color.Red;
            Color remapColor = Color.White;
            Color foundationLineColor = Color.White;
            string iniName = string.Empty;
            ObjectImage bibGraphics = null;

            int yDrawPointWithoutCellHeight = drawPoint.Y;

            var mapCell = Map.GetTile(gameObject.Position);
            int heightOffset = mapCell.Level * Constants.CellHeight;
            if (mapCell != null)
                drawPoint -= new Point2D(0, heightOffset);

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
                    bibGraphics = TheaterGraphics.BuildingBibTextures[structure.ObjectType.Index];
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
                    int tiberiumIndex = overlay.OverlayType.GetTiberiumIndex();
                    if (tiberiumIndex > -1 && tiberiumIndex < Map.Rules.TiberiumTypes.Count)
                        remapColor = Map.Rules.TiberiumTypes[tiberiumIndex].XNAColor;
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

            int frameIndex = -1;

            if (graphics != null && graphics.Frames != null && graphics.Frames.Length > 0)
            {
                frameIndex = gameObject.GetFrameIndex(graphics.Frames.Length);
            }

            PositionedTexture frame = null;

            int yDrawOffset = gameObject.GetYDrawOffset();
            int finalDrawPointX = drawPoint.X;
            int finalDrawPointRight = finalDrawPointX;
            int finalDrawPointY = drawPoint.Y;
            int finalDrawPointBottom = finalDrawPointY;
            int objectYDrawPointWithoutCellHeight = yDrawPointWithoutCellHeight;

            if (frameIndex > -1 && frameIndex < graphics.Frames.Length)
            {
                frame = graphics.Frames[frameIndex];

                if (frame != null)
                {
                    finalDrawPointX = drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2;
                    finalDrawPointY = drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;
                    objectYDrawPointWithoutCellHeight = yDrawPointWithoutCellHeight - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;

                    finalDrawPointRight = finalDrawPointX + frame.Texture.Width;
                    finalDrawPointBottom = finalDrawPointY + frame.Texture.Height;
                }
            }

            // If the object is not in view, skip
            if (finalDrawPointRight < Camera.TopLeftPoint.X || finalDrawPointX > GetCameraRightXCoord())
                return;

            if (finalDrawPointBottom < Camera.TopLeftPoint.Y || finalDrawPointY > GetCameraBottomYCoord())
                return;

            if (gameObject.WhatAmI() == RTTIType.Building)
            {
                var structure = (Structure)gameObject;
                int foundationX = structure.ObjectType.ArtConfig.FoundationX;
                int foundationY = structure.ObjectType.ArtConfig.FoundationY;
                if (foundationX > 0 && foundationY > 0)
                {
                    Point2D p1 = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, Map) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p2 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y), Map) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p3 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X, gameObject.Position.Y + foundationY), Map) + new Point2D(Constants.CellSizeX / 2, 0);
                    Point2D p4 = CellMath.CellTopLeftPointFromCellCoords(new Point2D(gameObject.Position.X + foundationX, gameObject.Position.Y + foundationY), Map) + new Point2D(Constants.CellSizeX / 2, 0);

                    p1 -= new Point2D(0, heightOffset);
                    p2 -= new Point2D(0, heightOffset);
                    p3 -= new Point2D(0, heightOffset);
                    p4 -= new Point2D(0, heightOffset);

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

            float depthTop = 0f;
            float depthBottom = 0f;
            int depthOffset = Constants.CellSizeY;
            Texture2D texture = null;
            Vector2 worldTextureCoordinates;
            Vector2 spriteSizeToWorldSizeRatio;

            // Draw building bib
            if (bibGraphics != null)
            {
                PositionedTexture bibFrame = bibGraphics.Frames[0];
                texture = bibFrame.Texture;

                int bibFinalDrawPointX = drawPoint.X - bibFrame.ShapeWidth / 2 + bibFrame.OffsetX + Constants.CellSizeX / 2;
                int bibFinalDrawPointY = drawPoint.Y - bibFrame.ShapeHeight / 2 + bibFrame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;
                int bibYDrawPointWithoutCellHeight = yDrawPointWithoutCellHeight - bibFrame.ShapeHeight / 2 + bibFrame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;

                depthTop = (bibYDrawPointWithoutCellHeight + depthOffset) / (float)Map.HeightInPixels;
                depthBottom = (bibYDrawPointWithoutCellHeight + texture.Height + depthOffset) / (float)Map.HeightInPixels;
                depthTop = 1.0f - depthTop;
                depthBottom = 1.0f - depthBottom;

                worldTextureCoordinates = new Vector2(bibFinalDrawPointX / (float)Map.WidthInPixels, bibFinalDrawPointY / (float)Map.HeightInPixels);
                spriteSizeToWorldSizeRatio = new Vector2(texture.Width / (float)Map.WidthInPixels, texture.Height / (float)Map.HeightInPixels);

                if (isRenderingDepth)
                    SetEffectParams(depthApplyEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, null);
                else
                    SetEffectParams(colorDrawEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTarget);

                DrawTexture(texture, new Rectangle(
                    bibFinalDrawPointX, bibFinalDrawPointY,
                    texture.Width, texture.Height),
                    null, Constants.HQRemap ? Color.White : remapColor,
                    0f, Vector2.Zero, SpriteEffects.None, 0f);

                if (Constants.HQRemap && bibGraphics.RemapFrames != null)
                {
                    DrawTexture(bibGraphics.RemapFrames[0].Texture,
                        new Rectangle(bibFinalDrawPointX, bibFinalDrawPointY, texture.Width, texture.Height),
                        null,
                        remapColor,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f);
                }
            }

            // Draw shadow
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
                            shadowTexture.Width, shadowTexture.Height),
                            null,
                            new Color(0, 0, 0, 128),
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0f);
                    }
                }
            }

            // We're going to render this object, update its last refresh frame
            // gameObject.LastRefreshIndex = refreshIndex;

            if (frame == null)
                return;

            texture = frame.Texture;

            depthTop = (objectYDrawPointWithoutCellHeight + depthOffset) / (float)Map.HeightInPixels;
            depthBottom = (objectYDrawPointWithoutCellHeight + texture.Height + depthOffset) / (float)Map.HeightInPixels;
            depthTop = 1.0f - depthTop;
            depthBottom = 1.0f - depthBottom;

            worldTextureCoordinates = new Vector2(finalDrawPointX / (float)Map.WidthInPixels, finalDrawPointY / (float)Map.HeightInPixels);
            spriteSizeToWorldSizeRatio = new Vector2(texture.Width / (float)Map.WidthInPixels, texture.Height / (float)Map.HeightInPixels);

            if (isRenderingDepth)
                SetEffectParams(depthApplyEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, null);
            else
                SetEffectParams(colorDrawEffect, depthBottom, depthTop, worldTextureCoordinates, spriteSizeToWorldSizeRatio, depthRenderTarget);

            DrawTexture(texture, new Rectangle(
                finalDrawPointX, finalDrawPointY,
                texture.Width, texture.Height),
                null, Constants.HQRemap ? Color.White : remapColor,
                0f, Vector2.Zero, SpriteEffects.None, 0f);

            if (Constants.HQRemap && graphics.RemapFrames != null)
            {
                DrawTexture(graphics.RemapFrames[frameIndex].Texture,
                    new Rectangle(finalDrawPointX, finalDrawPointY, texture.Width, texture.Height),
                    null,
                    remapColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
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
                            texture.Width, texture.Height),
                            null,
                            Constants.HQRemap ? Color.White : remapColor,
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0f);

                        if (Constants.HQRemap && graphics.RemapFrames != null)
                        {
                            DrawTexture(graphics.RemapFrames[turretFrameIndex].Texture, new Rectangle(drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2,
                                drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset,
                                texture.Width, texture.Height),
                                null,
                                remapColor,
                                0f,
                                Vector2.Zero,
                                SpriteEffects.None,
                                0f);
                        }
                    }
                }
            }
        }

        private void DrawBaseNode(GraphicalBaseNode graphicalBaseNode)
        {
            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords_3D(graphicalBaseNode.BaseNode.Position, Map);

            ObjectImage bibGraphics = TheaterGraphics.BuildingBibTextures[graphicalBaseNode.BuildingType.Index];
            ObjectImage graphics = TheaterGraphics.BuildingTextures[graphicalBaseNode.BuildingType.Index];
            Color replacementColor = Color.Yellow;
            string iniName = graphicalBaseNode.BuildingType.ININame;
            Color remapColor = graphicalBaseNode.BuildingType.ArtConfig.Remapable ? graphicalBaseNode.Owner.XNAColor : Color.White;

            const float opacity = 0.25f;

            remapColor *= opacity;

            Color nonRemapBaseNodeShade = new Color(opacity, opacity, opacity * 2.0f, opacity * 2.0f);

            if (graphics == null || graphics.Frames.Length == 0)
            {
                DrawStringWithShadow(iniName, 1, drawPoint.ToXNAVector(), replacementColor, 1.0f);
                return;
            }

            int yDrawOffset = Constants.CellSizeY / -2;
            int frameIndex = 0;
            Texture2D texture;

            if (bibGraphics != null)
            {
                PositionedTexture bibFrame = bibGraphics.Frames[0];
                texture = bibFrame.Texture;

                int bibFinalDrawPointX = drawPoint.X - bibFrame.ShapeWidth / 2 + bibFrame.OffsetX + Constants.CellSizeX / 2;
                int bibFinalDrawPointY = drawPoint.Y - bibFrame.ShapeHeight / 2 + bibFrame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;

                DrawTexture(texture, new Rectangle(
                    bibFinalDrawPointX, bibFinalDrawPointY,
                    texture.Width, texture.Height),
                    null, Constants.HQRemap ? nonRemapBaseNodeShade : remapColor,
                    0f, Vector2.Zero, SpriteEffects.None, 0f);

                if (Constants.HQRemap && bibGraphics.RemapFrames != null)
                {
                    DrawTexture(bibGraphics.RemapFrames[0].Texture,
                        new Rectangle(bibFinalDrawPointX, bibFinalDrawPointY, texture.Width, texture.Height),
                        null,
                        remapColor,
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f);
                }
            }

            var frame = graphics.Frames[frameIndex];
            if (frame == null)
                return;

            texture = frame.Texture;

            int x = drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2;
            int y = drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;
            int width = texture.Width;
            int height = texture.Height;
            Rectangle drawRectangle = new Rectangle(x, y, width, height);

            DrawTexture(texture, drawRectangle, Constants.HQRemap ? nonRemapBaseNodeShade : remapColor);

            if (Constants.HQRemap && graphics.RemapFrames != null)
            {
                DrawTexture(graphics.RemapFrames[frameIndex].Texture, drawRectangle, remapColor);
            }
        }

        private void DrawWaypoint(Waypoint waypoint)
        {
            const int waypointBorderOffsetX = 8;
            const int waypointBorderOffsetY = 4;
            const int textOffset = 3;

            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(waypoint.Position, Map);

            var cell = Map.GetTile(waypoint.Position);
            if (cell != null)
                drawPoint -= new Point2D(0, cell.Level * Constants.CellHeight);

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

            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(cellTag.Position, Map);

            var rect = new Rectangle(drawPoint.X + cellTagBorderOffsetX,
                drawPoint.Y + cellTagBorderOffsetY,
                Constants.CellSizeX - (cellTagBorderOffsetX * 2),
                Constants.CellSizeY - (cellTagBorderOffsetY * 2));

            FillRectangle(rect, new Color(128, 0, 0, 128));
            DrawRectangle(rect, Color.Yellow);
        }

        private void DrawMapBorder()
        {
            const int BorderThickness = 4;
            const double InitialHeight = 2.5; // TS engine assumes that the first cell is at a height of 2.5
            const int HeightAddition = 4; // TS engine adds 4 to specified map height <3
            const int TopImpassableCellCount = 3; // The northernmost 3 cells are impassable in the TS engine, we'll also display this border

            int x = (int)((Map.LocalSize.X * Constants.CellSizeX - Camera.TopLeftPoint.X) * Camera.ZoomLevel);
            int y = (int)((((Map.LocalSize.Y - InitialHeight) * Constants.CellSizeY) - Camera.TopLeftPoint.Y) * Camera.ZoomLevel);
            int width = (int)((Map.LocalSize.Width * Constants.CellSizeX) * Camera.ZoomLevel);
            int height = (int)((Map.LocalSize.Height + HeightAddition) * Constants.CellSizeY * Camera.ZoomLevel);

            DrawRectangle(new Rectangle(x, y, width, height), Color.Blue, BorderThickness);

            int impassableY = (int)(y + (Constants.CellSizeY * TopImpassableCellCount * Camera.ZoomLevel));
            FillRectangle(new Rectangle(x, impassableY - (BorderThickness / 2), width, BorderThickness), Color.Teal * 0.25f);
        }

        public override void OnMouseScrolled()
        {
            if (Cursor.ScrollWheelValue > 0)
                Camera.ZoomLevel += 0.1;
            else
                Camera.ZoomLevel -= 0.1;

            base.OnMouseScrolled();
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
                    float rightClickScrollRate = (float)((scrollRate / RightClickScrollRateDivisor) / Camera.ZoomLevel);

                    Camera.FloatTopLeftPoint = new Vector2(Camera.FloatTopLeftPoint.X + result.X * rightClickScrollRate,
                        Camera.FloatTopLeftPoint.Y + result.Y * rightClickScrollRate);
                }
            }

            base.OnMouseOnControl();
        }

        public override void OnMouseEnter()
        {
            if (isRightClickScrolling)
                rightClickScrollInitPos = GetCursorPoint();

            base.OnMouseEnter();
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
                    Camera.FloatTopLeftPoint = Camera.TopLeftPoint.ToXNAVector();
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
                Camera.KeyboardUpdate(Keyboard, scrollRate);
            }

            windowController.MinimapWindow.CameraRectangle = new Rectangle(Camera.TopLeftPoint.ToXNAPoint(), new Point2D(Width, Height).ScaleBy(1.0 / Camera.ZoomLevel).ToXNAPoint());

            Point2D cursorMapPoint = GetCursorMapPoint();
            Point2D tileCoords = CellMath.CellCoordsFromPixelCoords(cursorMapPoint, Map);
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

        private Point2D GetCursorMapPoint()
        {
            Point cursorPoint = GetCursorPoint();
            Point2D cursorMapPoint = new Point2D(Camera.TopLeftPoint.X + (int)(cursorPoint.X / Camera.ZoomLevel),
                    Camera.TopLeftPoint.Y + (int)(cursorPoint.Y / Camera.ZoomLevel));

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

                Point2D cameraAndCellCenterOffset = new Point2D(-Camera.TopLeftPoint.X + Constants.CellSizeX / 2,
                                                 -Camera.TopLeftPoint.Y + Constants.CellSizeY / 2);

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map) + cameraAndCellCenterOffset;

                var startCell = Map.GetTile(draggedOrRotatedObject.Position);
                if (startCell != null)
                    startDrawPoint -= new Point2D(0, Map.GetTile(draggedOrRotatedObject.Position).Level * Constants.CellHeight);

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map) + cameraAndCellCenterOffset;

                endDrawPoint -= new Point2D(0, tileUnderCursor.Level * Constants.CellHeight);

                startDrawPoint = startDrawPoint.ScaleBy(Camera.ZoomLevel);
                endDrawPoint = endDrawPoint.ScaleBy(Camera.ZoomLevel);

                DrawLine(startDrawPoint.ToXNAVector(), endDrawPoint.ToXNAVector(), lineColor, 1);
            }
            else if (isRotatingObject)
            {
                Color lineColor = Color.Yellow;

                Point2D cameraAndCellCenterOffset = new Point2D(-Camera.TopLeftPoint.X + Constants.CellSizeX / 2,
                                                 -Camera.TopLeftPoint.Y + Constants.CellSizeY / 2);

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map) + cameraAndCellCenterOffset;

                var startCell = Map.GetTile(draggedOrRotatedObject.Position);
                if (startCell != null)
                    startDrawPoint -= new Point2D(0, Map.GetTile(draggedOrRotatedObject.Position).Level * Constants.CellHeight);

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map) + cameraAndCellCenterOffset;

                endDrawPoint -= new Point2D(0, tileUnderCursor.Level * Constants.CellHeight);

                startDrawPoint = startDrawPoint.ScaleBy(Camera.ZoomLevel);
                endDrawPoint = endDrawPoint.ScaleBy(Camera.ZoomLevel);

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
            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(tileUnderCursor.X, tileUnderCursor.Y), Map) - Camera.TopLeftPoint;

            int height = tileUnderCursor.Level * Constants.CellHeight;

            cellTopLeftPoint = new Point2D((int)(cellTopLeftPoint.X * Camera.ZoomLevel), (int)((cellTopLeftPoint.Y - height) * Camera.ZoomLevel));

            var cellTopPoint = new Vector2(cellTopLeftPoint.X + (int)((Constants.CellSizeX / 2) * Camera.ZoomLevel), cellTopLeftPoint.Y);
            var cellLeftPoint = new Vector2(cellTopLeftPoint.X, cellTopLeftPoint.Y + (int)((Constants.CellSizeY / 2) * Camera.ZoomLevel));
            var cellRightPoint = new Vector2(cellTopLeftPoint.X + (int)(Constants.CellSizeX * Camera.ZoomLevel), cellLeftPoint.Y);
            var cellBottomPoint = new Vector2(cellTopPoint.X, cellTopLeftPoint.Y + (int)(Constants.CellSizeY * Camera.ZoomLevel));

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

            int zoomedHeight = (int)(height * Camera.ZoomLevel);

            Color heightBarColor = Color.Black * 0.25f;
            FillRectangle(new Rectangle((int)cellLeftPoint.X, (int)cellLeftPoint.Y, 1, zoomedHeight), heightBarColor);
            FillRectangle(new Rectangle((int)cellBottomPoint.X, (int)cellBottomPoint.Y, 1, zoomedHeight), heightBarColor);
            FillRectangle(new Rectangle((int)cellRightPoint.X, (int)cellRightPoint.Y, 1, zoomedHeight), heightBarColor);
        }

        private void DrawImpassableHighlight(MapTile cell)
        {
            if (!Helpers.IsLandTypeImpassable(TheaterGraphics.GetTileGraphics(cell.TileIndex).GetSubTile(cell.SubTileIndex).TmpImage.TerrainType, false) && 
                (cell.Overlay == null || cell.Overlay.OverlayType == null || !Helpers.IsLandTypeImpassable(cell.Overlay.OverlayType.Land, false)))
            {
                return;
            }

            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords_3D(cell.CoordsToPoint(), Map);

            DrawTexture(impassableCellHighlightTexture, cellTopLeftPoint.ToXNAPoint(), Color.White);
        }

        private void DrawIceGrowthHighlight(MapTile cell)
        {
            if (cell.IceGrowth <= 0)
                return;

            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cell.CoordsToPoint(), Map);

            DrawTexture(iceGrowthHighlightTexture, cellTopLeftPoint.ToXNAPoint(), Color.White);
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
                var entryCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.EntryPoint, Map);
                var exitCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.ExitPoint, Map);
                var entryCell = Map.GetTile(tube.EntryPoint);
                int height = 0;
                if (entryCell != null)
                    height = entryCell.Level * Constants.CellHeight;

                Point2D currentPoint = tube.EntryPoint;

                Color color = tube.Pending ? Color.Orange : Color.LimeGreen;

                foreach (var direction in tube.Directions)
                {
                    Point2D nextPoint = currentPoint.NextPointFromTubeDirection(direction);

                    if (nextPoint != currentPoint)
                    {
                        var currentPixelPoint = CellMath.CellCenterPointFromCellCoords(currentPoint, Map);
                        var nextPixelPoint = CellMath.CellCenterPointFromCellCoords(nextPoint, Map);

                        DrawArrow(currentPixelPoint.ToXNAVector() - new Vector2(0, height),
                            nextPixelPoint.ToXNAVector() - new Vector2(0, height),
                            color, 0.25f, 10f, 1);
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
            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PreMapDraw(tileUnderCursor.CoordsToPoint());
            }

            if (mapInvalidated || cameraMoved)
            {
                DrawVisibleMapPortion();
                mapInvalidated = false;
                cameraMoved = false;
                DrawTubes();
            }

            DrawWorld();

            if (EditorState.DrawMapWideOverlay)
            {
                mapWideOverlay.Draw(new Rectangle(
                        (int)(-Camera.TopLeftPoint.X * Camera.ZoomLevel),
                        (int)(-Camera.TopLeftPoint.Y * Camera.ZoomLevel),
                        (int)(mapRenderTarget.Width * Camera.ZoomLevel),
                        (int)(mapRenderTarget.Height * Camera.ZoomLevel)));
            }

            DrawMapBorder();

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PostMapDraw(tileUnderCursor.CoordsToPoint());
                CursorAction.DrawPreview(tileUnderCursor.CoordsToPoint(), Camera.TopLeftPoint);
            }

            DrawOnTileUnderCursor();

            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            int zoomedWidth = (int)(Width / Camera.ZoomLevel);
            int zoomedHeight = (int)(Height / Camera.ZoomLevel);

            // Constrain draw coordinates so that we don't draw out of bounds and cause weird artifacts on map edge

            int sourceX = Camera.TopLeftPoint.X;
            int destinationX = 0;
            int destinationWidth = Width;
            if (sourceX < 0)
            {
                sourceX = 0;
                destinationX = (int)(-Camera.TopLeftPoint.X * Camera.ZoomLevel);
                destinationWidth -= destinationX;
                zoomedWidth += Camera.TopLeftPoint.X;
            }

            int sourceY = Camera.TopLeftPoint.Y;
            int destinationY = 0;
            int destinationHeight = Height;
            if (sourceY < 0)
            {
                sourceY = 0;
                destinationY = (int)(-Camera.TopLeftPoint.Y * Camera.ZoomLevel);
                destinationHeight -= destinationY;
                zoomedHeight += Camera.TopLeftPoint.Y;
            }

            if (sourceX + zoomedWidth > mapRenderTarget.Width)
            {
                zoomedWidth = mapRenderTarget.Width - sourceX;
                destinationWidth = (int)(zoomedWidth * Camera.ZoomLevel);
            }

            if (sourceY + zoomedHeight > mapRenderTarget.Height)
            {
                zoomedHeight = mapRenderTarget.Height - sourceY;
                destinationHeight = (int)(zoomedHeight * Camera.ZoomLevel);
            }

            Renderer.PushRenderTarget(compositeRenderTarget, new SpriteBatchSettings(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null));

            GraphicsDevice.Clear(Color.Black);

            // Rectangle sourceRectangle = new Rectangle(sourceX, sourceY, zoomedWidth, zoomedHeight);
            // Rectangle destinationRectangle = new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight);
            Rectangle sourceRectangle = new Rectangle(0, 0, mapRenderTarget.Width, mapRenderTarget.Height);
            Rectangle destinationRectangle = sourceRectangle;

            DrawTexture(mapRenderTarget,
                sourceRectangle,
                destinationRectangle,
                Color.White);

            DrawTexture(objectRenderTarget,
                sourceRectangle,
                destinationRectangle,
                Color.White);

            if (debugRenderDepthBuffer)
            {
                DrawTexture(depthRenderTarget,
                    sourceRectangle, destinationRectangle,
                    Color.White);
            }

            DrawTexture(transparencyRenderTarget,
                sourceRectangle,
                destinationRectangle,
                Color.White);

            Renderer.PopRenderTarget();

            // Draw directly to the screen
            sourceRectangle = new Rectangle(sourceX, sourceY, zoomedWidth, zoomedHeight);
            destinationRectangle = new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight);

            Renderer.PushSettings(new SpriteBatchSettings(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null));

            DrawTexture(compositeRenderTarget,
                sourceRectangle,
                destinationRectangle,
                Color.White);

            Renderer.PopSettings();
        }
    }
}
