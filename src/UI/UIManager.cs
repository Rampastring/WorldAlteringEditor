using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Sidebar;

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
        private EditorSidebar editorSidebar;

        private EditorState editorState;
        private TileInfoDisplay tileInfoDisplay;

        private TerrainPlacementAction terrainPlacementAction;
        private ChangeTechnoOwnerAction changeTechnoOwnerAction;

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

            editorState = new EditorState();
            mutationManager = new MutationManager();

            mapView = new MapView(WindowManager, map, theaterGraphics, editorState, mutationManager);
            mapView.Width = WindowManager.RenderResolutionX;
            mapView.Height = WindowManager.RenderResolutionY;
            AddChild(mapView);

            terrainPlacementAction = new TerrainPlacementAction(mapView);
            changeTechnoOwnerAction = new ChangeTechnoOwnerAction(mapView);
            editorState.ObjectOwnerChanged += (s, e) => editorState.CursorAction = changeTechnoOwnerAction;

            editorSidebar = new EditorSidebar(WindowManager, editorState, map, theaterGraphics, mapView);
            editorSidebar.Width = 250;
            editorSidebar.Y = 40;
            editorSidebar.Height = WindowManager.RenderResolutionY - editorSidebar.Y;
            AddChild(editorSidebar);

            tileSelector = new TileSelector(WindowManager, theaterGraphics);
            tileSelector.X = editorSidebar.Right;
            tileSelector.Width = WindowManager.RenderResolutionX - tileSelector.X;
            tileSelector.Height = 300;
            tileSelector.Y = WindowManager.RenderResolutionY - tileSelector.Height;
            
            AddChild(tileSelector);
            tileSelector.TileDisplay.SelectedTileChanged += TileDisplay_SelectedTileChanged;

            tileInfoDisplay = new TileInfoDisplay(WindowManager, theaterGraphics);
            AddChild(tileInfoDisplay);
            tileInfoDisplay.X = Width - tileInfoDisplay.Width;
            mapView.TileInfoDisplay = tileInfoDisplay;

            base.Initialize();

            if (map.Houses.Count > 0)
                editorState.ObjectOwner = map.Houses[0];

            Keyboard.OnKeyPressed += Keyboard_OnKeyPressed;

            editorState.BrushSize = map.EditorConfig.BrushSizes[0];
            KeyboardCommands.Instance.NextBrushSize.Triggered += NextBrushSize_Triggered;
            KeyboardCommands.Instance.PreviousBrushSize.Triggered += PreviousBrushSize_Triggered;
        }

        private void PreviousBrushSize_Triggered(object sender, EventArgs e)
        {
            int currentBrushSizeIndex = map.EditorConfig.BrushSizes.FindIndex(b => b == editorState.BrushSize);
            if (currentBrushSizeIndex < 1)
                return;

            editorState.BrushSize = map.EditorConfig.BrushSizes[currentBrushSizeIndex - 1];
        }

        private void NextBrushSize_Triggered(object sender, EventArgs e)
        {
            int currentBrushSizeIndex = map.EditorConfig.BrushSizes.FindIndex(b => b == editorState.BrushSize);
            if (currentBrushSizeIndex >= map.EditorConfig.BrushSizes.Count - 1)
                return;

            editorState.BrushSize = map.EditorConfig.BrushSizes[currentBrushSizeIndex + 1];
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
            mapView.CursorAction = terrainPlacementAction;
            terrainPlacementAction.Tile = tileSelector.TileDisplay.SelectedTile;
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
