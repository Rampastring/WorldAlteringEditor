using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Sidebar;
using TSMapEditor.UI.TopBar;
using TSMapEditor.UI.Windows;

namespace TSMapEditor.UI
{
    class UIManager : XNAControl
    {
        public UIManager(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
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

        private MutationManager mutationManager;


        public override void Initialize()
        {
            Name = nameof(UIManager);

            UISettings.ActiveSettings.PanelBackgroundColor = new Color(0, 0, 0, 128);
            UISettings.ActiveSettings.PanelBorderColor = new Color(128, 128, 128, 255);

            Width = WindowManager.RenderResolutionX;
            Height = WindowManager.RenderResolutionY;

            // Keyboard must be initialized before any other controls so it's properly usable
            KeyboardCommands.Instance = new KeyboardCommands();
            KeyboardCommands.Instance.Undo.Action = UndoAction;
            KeyboardCommands.Instance.Redo.Action = RedoAction;
            KeyboardCommands.Instance.ReadFromSettings();

            var windowController = new WindowController();
            editorState = new EditorState();
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

            tileSelector = new TileSelector(WindowManager, theaterGraphics);
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

            var topBarMenu = new TopBarMenu(WindowManager, mutationManager, map, windowController);
            AddChild(topBarMenu);
            topBarMenu.Width = editorSidebar.Width;

            var topBarControlMenu = new TopBarControlMenu(WindowManager, map, theaterGraphics,
                map.EditorConfig, editorState, placeTerrainCursorAction, placeWaypointCursorAction, deletionModeCursorAction);
            topBarControlMenu.X = topBarMenu.Right;
            topBarControlMenu.Width = 500;
            AddChild(topBarControlMenu);

            base.Initialize();

            windowController.Initialize(this, map, editorState, mapView);
            placeWaypointCursorAction.PlaceWaypointWindow = windowController.PlaceWaypointWindow;

            if (map.Houses.Count > 0)
                editorState.ObjectOwner = map.Houses[0];

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;

            editorState.CursorActionChanged += EditorState_CursorActionChanged;
            overlayPlacementAction.OverlayTypeChanged += OverlayPlacementAction_OverlayTypeChanged;
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

                    if ((keyboardCommand.Key.Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt && !Keyboard.IsAltHeldDown())
                        continue;

                    if ((keyboardCommand.Key.Modifiers & KeyboardModifiers.Ctrl) == KeyboardModifiers.Ctrl && !Keyboard.IsCtrlHeldDown())
                        continue;

                    if ((keyboardCommand.Key.Modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift && !Keyboard.IsShiftHeldDown())
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
    }
}
