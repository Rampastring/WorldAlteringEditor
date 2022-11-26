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
        private int MaxRefreshTimeInFrame = 24;
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
        private RenderTarget2D objectsRenderTarget;
        private RenderTarget2D transparencyRenderTarget;

        private MapTile tileUnderCursor;
        private MapTile lastTileUnderCursor;

        private bool mapInvalidated = true;

        private int refreshSizeSetting;
        private int scrollRate;

        private bool isDraggingObject = false;
        private bool isRotatingObject = false;
        private IMovable draggedOrRotatedObject = null;

        // For right-click scrolling
        private bool isRightClickScrolling = false;
        private Point rightClickScrollInitPos = new Point(-1, -1);

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

        private Stopwatch refreshStopwatch;

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

            impassableCellHighlightTexture = AssetLoader.LoadTexture("impassablehighlight.png");
            iceGrowthHighlightTexture = AssetLoader.LoadTexture("icehighlight.png");

            const string MapWideOverlayTextureName = "mapwideoverlay.png";
            if (AssetLoader.AssetExists(MapWideOverlayTextureName))
                mapWideOverlayTexture = AssetLoader.LoadTexture(MapWideOverlayTextureName);
            EditorState.MapWideOverlayExists = mapWideOverlayTexture != null;
            mapWideOverlayTextureOpacity = UserSettings.Instance.MapWideOverlayOpacity / 255.0f;

            mapRenderTarget = CreateFullMapRenderTarget();
            objectsRenderTarget = CreateFullMapRenderTarget();
            transparencyRenderTarget = CreateFullMapRenderTarget();

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

            Map.HouseColorChanged += (s, e) =>
            {
                Map.DoForAllTechnos(obj => { if (obj.Owner == e.House) AddRefreshPoint(obj.Position, 0); });
            };

            EditorState.HighlightImpassableCellsChanged += (s, e) => InvalidateMap();
            EditorState.HighlightIceGrowthChanged += (s, e) => InvalidateMap();

            KeyboardCommands.Instance.RotateUnitOneStep.Triggered += RotateUnitOneStep_Triggered;

            refreshStopwatch = new Stopwatch();
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
            transparencyRenderTarget?.Dispose();

            mapRenderTarget = CreateFullMapRenderTarget();
            transparencyRenderTarget = CreateFullMapRenderTarget();

            windowController.MinimapWindow.MegamapTexture = mapRenderTarget;

            // And then re-draw the whole map
            InvalidateMap();
        }

        private void MinimapWindow_MegamapClicked(object sender, MegamapClickedEventArgs e)
        {
            Camera.TopLeftPoint = e.ClickedPoint - new Point2D(Width / 2, Height / 2).ScaleBy(1.0 / Camera.ZoomLevel);
        }

        private void FrameworkMode_Triggered(object sender, EventArgs e)
        {
            EditorState.IsMarbleMadness = !EditorState.IsMarbleMadness;
            mapInvalidated = true;
        }

        private void EditorState_CursorActionChanged(object sender, EventArgs e)
        {
            if (lastTileUnderCursor != null)
                AddRefreshPoint(lastTileUnderCursor.CoordsToPoint(), 3);

            lastTileUnderCursor = null;
        }

        private RenderTarget2D CreateFullMapRenderTarget()
        {
           return new RenderTarget2D(GraphicsDevice,
               Map.Size.X * Constants.CellSizeX,
               Map.Size.Y * Constants.CellSizeY + Constants.CellSizeY / 2, false, SurfaceFormat.Color,
               DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);
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
                    if (row[j] == null)
                    {
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

            DrawTubes();

            if (EditorState.HighlightImpassableCells)
            {
                Map.DoForAllValidTiles(cell => DrawImpassableHighlight(cell));
            }

            if (EditorState.HighlightIceGrowth)
            {
                Map.DoForAllValidTiles(cell => DrawIceGrowthHighlight(cell));
            }

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

        private void DrawBaseNode(GraphicalBaseNode graphicalBaseNode)
        {
            Point2D drawPoint = CellMath.CellTopLeftPointFromCellCoords(graphicalBaseNode.BaseNode.Location, Map.Size.X);

            ObjectImage graphics = TheaterGraphics.BuildingTextures[graphicalBaseNode.BuildingType.Index];
            Color replacementColor = Color.Yellow;
            string iniName = graphicalBaseNode.BuildingType.ININame;
            Color remapColor = graphicalBaseNode.BuildingType.ArtConfig.Remapable ? graphicalBaseNode.Owner.XNAColor : Color.White;
            Color foundationLineColor = graphicalBaseNode.Owner.XNAColor;

            if (graphics == null || graphics.Frames.Length == 0)
            {
                DrawStringWithShadow(iniName, 1, drawPoint.ToXNAVector(), replacementColor, 1.0f);
                return;
            }

            int yDrawOffset = Constants.CellSizeY / -2;
            int frameIndex = 0;


            var frame = graphics.Frames[frameIndex];
            if (frame == null)
                return;

            var texture = frame.Texture;

            int x = drawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2;
            int y = drawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;
            int width = texture.Width;
            int height = texture.Height;
            Rectangle drawRectangle = new Rectangle(x, y, width, height);

            DrawTexture(texture, drawRectangle, Constants.HQRemap ? Color.White : remapColor);

            if (Constants.HQRemap && graphics.RemapFrames != null)
            {
                DrawTexture(graphics.RemapFrames[frameIndex].Texture, drawRectangle, remapColor);
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

            MGTMPImage tmpImage = tileImage.TMPImages[subTileIndex];

            if (tmpImage.Texture != null)
            {
                DrawTexture(tmpImage.Texture, new Rectangle(drawPoint.X, drawPoint.Y,
                    Constants.CellSizeX, Constants.CellSizeY), Color.White);
            }

            if (tmpImage.ExtraTexture != null)
            {
                DrawTexture(tmpImage.ExtraTexture,
                    new Rectangle(drawPoint.X + tmpImage.TmpImage.XExtra,
                    drawPoint.Y + tmpImage.TmpImage.YExtra,
                    tmpImage.ExtraTexture.Width,
                    tmpImage.ExtraTexture.Height),
                    Color.White);
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

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map.Size.X) + cameraAndCellCenterOffset;

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map.Size.X) + cameraAndCellCenterOffset;

                startDrawPoint = startDrawPoint.ScaleBy(Camera.ZoomLevel);
                endDrawPoint = endDrawPoint.ScaleBy(Camera.ZoomLevel);

                DrawLine(startDrawPoint.ToXNAVector(), endDrawPoint.ToXNAVector(), lineColor, 1);
            }
            else if (isRotatingObject)
            {
                Color lineColor = Color.Yellow;

                Point2D cameraAndCellCenterOffset = new Point2D(-Camera.TopLeftPoint.X + Constants.CellSizeX / 2,
                                                 -Camera.TopLeftPoint.Y + Constants.CellSizeY / 2);

                Point2D startDrawPoint = CellMath.CellTopLeftPointFromCellCoords(draggedOrRotatedObject.Position, Map.Size.X) + cameraAndCellCenterOffset;

                Point2D endDrawPoint = CellMath.CellTopLeftPointFromCellCoords(tileUnderCursor.CoordsToPoint(), Map.Size.X) + cameraAndCellCenterOffset;

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
            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(new Point2D(tileUnderCursor.X, tileUnderCursor.Y), Map.Size.X) - Camera.TopLeftPoint;

            cellTopLeftPoint = new Point2D((int)(cellTopLeftPoint.X * Camera.ZoomLevel), (int)(cellTopLeftPoint.Y * Camera.ZoomLevel));

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
        }

        private void DrawImpassableHighlight(MapTile cell)
        {
            if (!Helpers.IsLandTypeImpassable(TheaterGraphics.GetTileGraphics(cell.TileIndex).GetSubTile(cell.SubTileIndex).TmpImage.TerrainType, false) && 
                (cell.Overlay == null || cell.Overlay.OverlayType == null || !Helpers.IsLandTypeImpassable(cell.Overlay.OverlayType.Land, false)))
            {
                return;
            }

            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cell.CoordsToPoint(), Map.Size.X);

            DrawTexture(impassableCellHighlightTexture, cellTopLeftPoint.ToXNAPoint(), Color.White);
        }

        private void DrawIceGrowthHighlight(MapTile cell)
        {
            if (cell.IceGrowth <= 0)
                return;

            Point2D cellTopLeftPoint = CellMath.CellTopLeftPointFromCellCoords(cell.CoordsToPoint(), Map.Size.X);

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
                var entryCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.EntryPoint, Map.Size.X);
                var exitCellCenterPoint = CellMath.CellCenterPointFromCellCoords(tube.ExitPoint, Map.Size.X);

                Point2D currentPoint = tube.EntryPoint;

                Color color = tube.Pending ? Color.Orange : Color.LimeGreen;

                foreach (var direction in tube.Directions)
                {
                    Point2D nextPoint = currentPoint.NextPointFromTubeDirection(direction);

                    if (nextPoint != currentPoint)
                    {
                        var currentPixelPoint = CellMath.CellCenterPointFromCellCoords(currentPoint, Map.Size.X);
                        var nextPixelPoint = CellMath.CellCenterPointFromCellCoords(nextPoint, Map.Size.X);

                        DrawArrow(currentPixelPoint.ToXNAVector(), nextPixelPoint.ToXNAVector(), color, 0.25f, 10f, 2);
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

                refreshStopwatch.Reset();
                refreshStopwatch.Start();
                int i = 0;
                while (i < newRefreshes.Count)
                {
                    if (refreshStopwatch.ElapsedMilliseconds > MaxRefreshTimeInFrame)
                        break;

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

                    if (EditorState.HighlightImpassableCells)
                    {
                        foreach (var cell in sortedCells)
                        {
                            DrawImpassableHighlight(cell);
                        }
                    }

                    if (EditorState.HighlightIceGrowth)
                    {
                        foreach (var cell in sortedCells)
                        {
                            DrawIceGrowthHighlight(cell);
                        }
                    }

                    newRefreshes.RemoveAt(i);
                }
                refreshStopwatch.Stop();

                DrawTubes();
                Renderer.PopRenderTarget();
            }


            // foreach (var refresh in refreshes)
            // {
            //     RefreshOverArea(refresh);
            // }
            // refreshes.Clear();

            DrawWorld();

            DrawMapBorder();

            if (IsActive && tileUnderCursor != null && CursorAction != null)
            {
                CursorAction.PostMapDraw(tileUnderCursor.CoordsToPoint());
                CursorAction.DrawPreview(tileUnderCursor.CoordsToPoint(), Camera.TopLeftPoint);
            }

            if (mapWideOverlayTexture != null && EditorState.DrawMapWideOverlay)
            {
                Renderer.DrawTexture(mapWideOverlayTexture,
                    new Rectangle(
                        (int)(-Camera.TopLeftPoint.X * Camera.ZoomLevel),
                        (int)(-Camera.TopLeftPoint.Y * Camera.ZoomLevel),
                        (int)(mapRenderTarget.Width * Camera.ZoomLevel),
                        (int)(mapRenderTarget.Height * Camera.ZoomLevel)),
                    Color.White * mapWideOverlayTextureOpacity);
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

            Renderer.PushSettings(new SpriteBatchSettings(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null));

            DrawTexture(mapRenderTarget,
                new Rectangle(sourceX, sourceY, zoomedWidth, zoomedHeight),
                new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight),
                Color.White);

            Renderer.PushRenderTarget(transparencyRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            Map.GraphicalBaseNodes.ForEach(bn => DrawBaseNode(bn));
            Renderer.PopRenderTarget();

            DrawTexture(transparencyRenderTarget,
                new Rectangle(sourceX, sourceY, zoomedWidth, zoomedHeight),
                new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight),
                new Color(128, 128, 255) * 0.5f);

            Renderer.PopSettings();
        }
    }
}
