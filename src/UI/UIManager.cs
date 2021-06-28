using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.Models;
using TSMapEditor.Rendering;
using TSMapEditor.UI.CursorActions;

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
        private EditorState editorState;
        private TileInfoDisplay tileInfoDisplay;

        private TerrainPlacementAction terrainPlacementAction;

        public override void Initialize()
        {
            Name = nameof(UIManager);

            UISettings.ActiveSettings.PanelBackgroundColor = new Color(0, 0, 0, 128);
            UISettings.ActiveSettings.PanelBorderColor = new Color(128, 128, 128, 255);

            Width = WindowManager.RenderResolutionX;
            Height = WindowManager.RenderResolutionY;

            editorState = new EditorState();
            terrainPlacementAction = new TerrainPlacementAction();

            mapView = new MapView(WindowManager, map, theaterGraphics, new EditorState());
            mapView.Width = WindowManager.RenderResolutionX;
            mapView.Height = WindowManager.RenderResolutionY;
            AddChild(mapView);

            tileSelector = new TileSelector(WindowManager, theaterGraphics);
            tileSelector.Width = WindowManager.RenderResolutionX;
            tileSelector.Height = 300;
            tileSelector.Y = WindowManager.RenderResolutionY - tileSelector.Height;
            AddChild(tileSelector);
            tileSelector.TileDisplay.SelectedTileChanged += TileDisplay_SelectedTileChanged;

            tileInfoDisplay = new TileInfoDisplay(WindowManager, theaterGraphics);
            AddChild(tileInfoDisplay);
            tileInfoDisplay.X = Width - tileInfoDisplay.Width;
            mapView.TileInfoDisplay = tileInfoDisplay;

            base.Initialize();
        }

        private void TileDisplay_SelectedTileChanged(object sender, EventArgs e)
        {
            mapView.CursorAction = terrainPlacementAction;
            terrainPlacementAction.Tile = tileSelector.TileDisplay.SelectedTile;
        }
    }
}
