﻿using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Linq;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Rendering;
using TSMapEditor.Settings;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Sidebar;
using TSMapEditor.UI.TopBar;
using TSMapEditor.UI.Windows;
using TSMapEditor.UI.Windows.MainMenuWindows;

namespace TSMapEditor.UI
{
    public class CustomUISettings : UISettings
    {
        public Color ListBoxBackgroundColor { get; set; } = Color.Black;
        public Color ButtonMainBackgroundColor { get; set; } = new Color(0, 0, 0, 196);
        public Color ButtonSecondaryBackgroundColor { get; set; } = new Color(0, 0, 0, 255);
        public Color ButtonTertiaryBackgroundColor { get; set; } = Color.White;
    }

    class UIManager : XNAControl, IWindowParentControl
    {
        public UIManager(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            DrawMode = ControlDrawMode.UNIQUE_RENDER_TARGET;
            this.map = map;
            this.theaterGraphics = theaterGraphics;
        }

        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;

        private MapView mapView;
        private TileSelector tileSelector;
        private OverlayFrameSelector overlayFrameSelector;
        private EditorSidebar editorSidebar;

        private EditorState editorState;
        private TileInfoDisplay tileInfoDisplay;

        private PlaceTerrainCursorAction placeTerrainCursorAction;
        private ChangeTechnoOwnerAction changeTechnoOwnerAction;
        private PlaceWaypointCursorAction placeWaypointCursorAction;
        private OverlayPlacementAction overlayPlacementAction;

        private WindowController windowController;

        private MutationManager mutationManager;

        private int loadMapStage;
        private string loadMapFilePath;
        private CreateNewMapEventArgs newMapInfo;
        private DarkeningPanel mapLoadDarkeningPanel;

        public void SetAutoUpdateChildOrder(bool value) => AutoUpdateChildOrder = value;


        public override void Initialize()
        {
            Name = nameof(UIManager);

            UISettings.ActiveSettings.PanelBackgroundColor = new Color(0, 0, 0, 128);
            UISettings.ActiveSettings.PanelBorderColor = new Color(128, 128, 128, 255);

            bool boldFont = UserSettings.Instance.UseBoldFont;
            if (boldFont)
            {
                Renderer.GetFontList()[0] = Renderer.GetFontList()[1];
            }

            bool lightTheme = false;
            if (lightTheme)
            {
                UISettings.ActiveSettings.TextShadowColor = Color.Gray;
                UISettings.ActiveSettings.TextShadowDistance = 0;
                ((CustomUISettings)UISettings.ActiveSettings).ListBoxBackgroundColor = Color.White * 0.77f;
                ((CustomUISettings)UISettings.ActiveSettings).ButtonMainBackgroundColor = Color.White * 0.77f;
                ((CustomUISettings)UISettings.ActiveSettings).ButtonSecondaryBackgroundColor = Color.Gray;
                ((CustomUISettings)UISettings.ActiveSettings).ButtonTertiaryBackgroundColor = Color.Black;
                UISettings.ActiveSettings.BackgroundColor = Color.White;
                UISettings.ActiveSettings.PanelBackgroundColor = Color.White;
                UISettings.ActiveSettings.TextColor = Color.Black;
                UISettings.ActiveSettings.FocusColor = Color.Gray;
                UISettings.ActiveSettings.AltColor = Color.Black;
                UISettings.ActiveSettings.ButtonTextColor = Color.Black;
            }

            bool greenTheme = UserSettings.Instance.Theme == "Tiberium";
            if (greenTheme)
            {
                // ((CustomUISettings)UISettings.ActiveSettings).ButtonSecondaryBackgroundColor = new Color(0, 164, 0);
                // ((CustomUISettings)UISettings.ActiveSettings).ButtonTertiaryBackgroundColor = Color.LimeGreen;
                UISettings.ActiveSettings.TextColor = new Color(0,222,0);
                UISettings.ActiveSettings.AltColor = Color.LimeGreen;
                UISettings.ActiveSettings.FocusColor = new Color(0, 96, 0);
                UISettings.ActiveSettings.ButtonTextColor = Color.LimeGreen;
                UISettings.ActiveSettings.PanelBorderColor = new Color(0, 164, 0);
            }

            Width = WindowManager.RenderResolutionX;
            Height = WindowManager.RenderResolutionY;

            // Keyboard must be initialized before any other controls so it's properly usable
            KeyboardCommands.Instance = new KeyboardCommands();
            KeyboardCommands.Instance.Undo.Action = UndoAction;
            KeyboardCommands.Instance.Redo.Action = RedoAction;
            KeyboardCommands.Instance.ReadFromSettings();

            windowController = new WindowController();
            editorState = new EditorState();
            editorState.BrushSize = map.EditorConfig.BrushSizes[0];
            mutationManager = new MutationManager();

            mapView = new MapView(WindowManager, map, theaterGraphics, editorState, mutationManager, windowController);
            mapView.Width = WindowManager.RenderResolutionX;
            mapView.Height = WindowManager.RenderResolutionY;
            AddChild(mapView);

            placeTerrainCursorAction = new PlaceTerrainCursorAction(mapView);
            placeWaypointCursorAction = new PlaceWaypointCursorAction(mapView);
            changeTechnoOwnerAction = new ChangeTechnoOwnerAction(mapView);
            var deletionModeCursorAction = new DeletionModeAction(mapView);
            editorState.ObjectOwnerChanged += (s, e) => editorState.CursorAction = changeTechnoOwnerAction;

            overlayPlacementAction = new OverlayPlacementAction(mapView);

            editorSidebar = new EditorSidebar(WindowManager, editorState, map, theaterGraphics, mapView, overlayPlacementAction);
            editorSidebar.Width = 250;
            editorSidebar.Y = Constants.UITopBarMenuHeight;
            editorSidebar.Height = WindowManager.RenderResolutionY - editorSidebar.Y;
            AddChild(editorSidebar);

            tileSelector = new TileSelector(WindowManager, theaterGraphics, placeTerrainCursorAction);
            tileSelector.X = editorSidebar.Right;
            tileSelector.Width = WindowManager.RenderResolutionX - tileSelector.X;
            tileSelector.Height = 300;
            tileSelector.Y = WindowManager.RenderResolutionY - tileSelector.Height;
            AddChild(tileSelector);
            tileSelector.TileDisplay.SelectedTileChanged += TileDisplay_SelectedTileChanged;
            tileSelector.ClientRectangleUpdated += UpdateTileAndOverlaySelectorArea;

            overlayFrameSelector = new OverlayFrameSelector(WindowManager, theaterGraphics, editorState);
            overlayFrameSelector.X = editorSidebar.Right;
            overlayFrameSelector.Width = tileSelector.Width;
            overlayFrameSelector.Height = tileSelector.Height;
            overlayFrameSelector.Y = tileSelector.Y;
            AddChild(overlayFrameSelector);
            overlayFrameSelector.SelectedFrameChanged += OverlayFrameSelector_SelectedFrameChanged;
            overlayFrameSelector.ClientRectangleUpdated += UpdateTileAndOverlaySelectorArea;
            overlayFrameSelector.Disable();

            tileInfoDisplay = new TileInfoDisplay(WindowManager, map, theaterGraphics);
            AddChild(tileInfoDisplay);
            tileInfoDisplay.X = Width - tileInfoDisplay.Width;
            mapView.TileInfoDisplay = tileInfoDisplay;

            var topBarMenu = new TopBarMenu(WindowManager, mutationManager, mapView, map, windowController);
            AddChild(topBarMenu);
            topBarMenu.Width = editorSidebar.Width;

            var topBarControlMenu = new TopBarControlMenu(WindowManager, map, theaterGraphics,
                map.EditorConfig, editorState, placeTerrainCursorAction, placeWaypointCursorAction, deletionModeCursorAction);
            topBarControlMenu.X = topBarMenu.Right;
            AddChild(topBarControlMenu);

            base.Initialize();

            windowController.Initialize(this, map, editorState, mapView);
            placeWaypointCursorAction.PlaceWaypointWindow = windowController.PlaceWaypointWindow;

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;

            editorState.CursorActionChanged += EditorState_CursorActionChanged;
            overlayPlacementAction.OverlayTypeChanged += OverlayPlacementAction_OverlayTypeChanged;

            windowController.OpenMapWindow.OnFileSelected += OpenMapWindow_OnFileSelected;
            windowController.CreateNewMapWindow.OnCreateNewMap += CreateNewMapWindow_OnCreateNewMap;

            // Try to select "Neutral" as default house
            editorState.ObjectOwner = map.GetHouses().Find(h => h.ININame == "Neutral");
            editorState.CursorAction = null;

            Alpha = 0f;

            map.MapWritten += Map_MapWritten;

            // This makes the exit process technically faster, but the editor stays longer on the
            // screen so practically increases exit time from the user's perspective
            // WindowManager.GameClosing += WindowManager_GameClosing;
        }

        private void WindowManager_GameClosing(object sender, EventArgs e)
        {
            ClearResources();
        }

        private void Map_MapWritten(object sender, EventArgs e)
        {
            var issues = map.CheckForIssues();

            if (issues.Count > 0)
            {
                if (issues.Count > 10)
                    issues = issues.Take(10).ToList();

                var newline = Environment.NewLine;

                string issuesString = string.Join(newline + newline, issues);

                EditorMessageBox.Show(WindowManager, "Issues Found",
                    "The map has been saved, but one or more issues have been found in the map. Please consider resolving them." + newline + newline + issuesString,
                    MessageBoxButtons.OK);
            }
        }

        private void CreateNewMapWindow_OnCreateNewMap(object sender, CreateNewMapEventArgs e)
        {
            loadMapFilePath = null;
            newMapInfo = e;
            StartLoadingMap();
        }

        private void OpenMapWindow_OnFileSelected(object sender, FileSelectedEventArgs e)
        {
            loadMapFilePath = e.FilePath;
            StartLoadingMap();
        }

        private void StartLoadingMap()
        {
            var messageBox = new EditorMessageBox(WindowManager, "Loading", "Please wait, loading map...", MessageBoxButtons.None);
            mapLoadDarkeningPanel = new DarkeningPanel(WindowManager);
            AddChild(mapLoadDarkeningPanel);
            mapLoadDarkeningPanel.AddChild(messageBox);

            loadMapStage = 1;
        }

        private void Clear()
        {
            map.Rules.TutorialLines.ShutdownFSW();
            windowController.OpenMapWindow.OnFileSelected -= OpenMapWindow_OnFileSelected;
            windowController.CreateNewMapWindow.OnCreateNewMap -= CreateNewMapWindow_OnCreateNewMap;
        }

        private void LoadMap()
        {
            bool createNew = loadMapFilePath == null;

            string error = MapSetup.InitializeMap(UserSettings.Instance.GameDirectory, createNew,
                loadMapFilePath,
                createNew ? newMapInfo.Theater : null,
                createNew ? newMapInfo.MapSize : Point2D.Zero,
                WindowManager);

            if (error != null)
            {
                EditorMessageBox.Show(WindowManager, "Failed to open map",
                    error, MessageBoxButtons.OK);
                loadMapStage = 0;
                RemoveChild(mapLoadDarkeningPanel);
                mapLoadDarkeningPanel.Kill();
                mapLoadDarkeningPanel = null;
                return;
            }

            if (!createNew)
                UserSettings.Instance.LastScenarioPath.UserDefinedValue = loadMapFilePath;

            ClearResources();
            WindowManager.RemoveControl(this);

            MapSetup.LoadTheaterGraphics(WindowManager, UserSettings.Instance.GameDirectory);
        }

        private void ClearResources()
        {
            // We need to free memory of everything that we've ever created and then load the map's theater graphics
            Clear();
            Disable();
            foreach (var child in Children)
                child.Disable();

            // TODO free up memory of textures created for controls - they should be mostly 
            // insignificant compared to the map textures though, so it shouldn't be too bad like this

            theaterGraphics.DisposeAll();
        }

        private void OverlayPlacementAction_OverlayTypeChanged(object sender, EventArgs e)
        {
            if (overlayPlacementAction.OverlayType == null)
            {
                ShowTileSelector();
                return;
            }

            ShowOverlayFrameSelector();
            overlayFrameSelector.SetOverlayType(overlayPlacementAction.OverlayType);
            return;
        }

        private void EditorState_CursorActionChanged(object sender, EventArgs e)
        {
            if (editorState.CursorAction == null)
            {
                ShowTileSelector();
                return;
            }

            var overlayPlacementAction = editorState.CursorAction as OverlayPlacementAction;
            if (overlayPlacementAction == null || overlayPlacementAction.OverlayType == null)
            {
                ShowTileSelector();
                return;
            }

            ShowOverlayFrameSelector();
        }

        private void ShowTileSelector()
        {
            tileSelector.Enable();
            overlayFrameSelector.Disable();
        }

        private void ShowOverlayFrameSelector()
        {
            tileSelector.Disable();
            overlayFrameSelector.Enable();
        }

        private void UpdateTileAndOverlaySelectorArea(object sender, EventArgs e)
        {
            tileSelector.ClientRectangleUpdated -= UpdateTileAndOverlaySelectorArea;
            overlayFrameSelector.ClientRectangleUpdated -= UpdateTileAndOverlaySelectorArea;

            if (sender == tileSelector)
                overlayFrameSelector.ClientRectangle = tileSelector.ClientRectangle;
            else
                tileSelector.ClientRectangle = overlayFrameSelector.ClientRectangle;

            tileSelector.ClientRectangleUpdated += UpdateTileAndOverlaySelectorArea;
            overlayFrameSelector.ClientRectangleUpdated += UpdateTileAndOverlaySelectorArea;
        }

        private void Keyboard_OnKeyPressed(object sender, Rampastring.XNAUI.Input.KeyPressEventArgs e)
        {
            if (!WindowManager.HasFocus)
                return;

            if (!IsActive)
                return;

            var selectedControl = WindowManager.SelectedControl;
            if (selectedControl != null)
            {
                if (selectedControl is XNATextBox || selectedControl is XNAListBox)
                    return;
            }

            foreach (var keyboardCommand in KeyboardCommands.Instance.Commands)
            {
                if (e.PressedKey == keyboardCommand.Key.Key)
                {
                    // Key matches, check modifiers

                    if (((keyboardCommand.Key.Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt) != Keyboard.IsAltHeldDown())
                        continue;

                    if (((keyboardCommand.Key.Modifiers & KeyboardModifiers.Ctrl) == KeyboardModifiers.Ctrl) != Keyboard.IsCtrlHeldDown())
                        continue;

                    if (((keyboardCommand.Key.Modifiers & KeyboardModifiers.Shift)) == KeyboardModifiers.Shift != Keyboard.IsShiftHeldDown())
                        continue;

                    // All keys match, perform the command!
                    e.Handled = true;

                    keyboardCommand.Action?.Invoke();
                    keyboardCommand.DoTrigger();
                    break;
                }
            }
        }

        private void TileDisplay_SelectedTileChanged(object sender, EventArgs e)
        {
            mapView.CursorAction = placeTerrainCursorAction;
            placeTerrainCursorAction.Tile = tileSelector.TileDisplay.SelectedTile;
            if (placeTerrainCursorAction.Tile != null)
            {
                // Check if this is a tile that is not benefical to place as larger than 1x1
                if (theaterGraphics.Theater.TileSets[placeTerrainCursorAction.Tile.TileSetId].Only1x1)
                    editorState.BrushSize = map.EditorConfig.BrushSizes.Find(bs => bs.Height == 1 && bs.Width == 1);
            }
        }

        private void OverlayFrameSelector_SelectedFrameChanged(object sender, EventArgs e)
        {
            if (overlayFrameSelector.SelectedFrameIndex < 0)
                overlayPlacementAction.FrameIndex = null;
            else
                overlayPlacementAction.FrameIndex = overlayFrameSelector.SelectedFrameIndex;
        }

        private void UndoAction()
        {
            mutationManager.Undo();
        }

        private void RedoAction()
        {
            mutationManager.Redo();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (loadMapStage > 0)
            {
                loadMapStage++;

                if (loadMapStage > 3)
                    LoadMap();
            }

            if (Alpha < 1.0f)
            {
                Alpha += (float)(gameTime.ElapsedGameTime.TotalSeconds * 1.0f);
            }
        }
    }
}
