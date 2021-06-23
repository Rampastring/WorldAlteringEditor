using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.CCEngine;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    public class TileSelector : XNAControl
    {
        private const int TileSetListWidth = 180;

        public TileSelector(WindowManager windowManager, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.theaterGraphics = theaterGraphics;
        }

        private TheaterGraphics theaterGraphics;
        private XNAListBox lbTileSetList;

        public TileDisplay TileDisplay { get; private set; }

        public override void Initialize()
        {
            Name = nameof(TileSelector);

            lbTileSetList = new XNAListBox(WindowManager);
            lbTileSetList.Name = nameof(lbTileSetList);
            lbTileSetList.Height = Height;
            lbTileSetList.Width = TileSetListWidth;
            lbTileSetList.SelectedIndexChanged += LbTileSetList_SelectedIndexChanged;
            AddChild(lbTileSetList);

            TileDisplay = new TileDisplay(WindowManager, theaterGraphics);
            TileDisplay.Name = nameof(TileDisplay);
            TileDisplay.Height = Height;
            TileDisplay.Width = Width - TileSetListWidth;
            TileDisplay.X = TileSetListWidth;
            AddChild(TileDisplay);

            lbTileSetList.BackgroundTexture = TileDisplay.BackgroundTexture;
            lbTileSetList.PanelBackgroundDrawMode = TileDisplay.PanelBackgroundDrawMode;

            base.Initialize();

            RefreshTileSets();
        }

        private void LbTileSetList_SelectedIndexChanged(object sender, EventArgs e)
        {
            TileSet tileSet = null;
            if (lbTileSetList.SelectedItem != null)
                tileSet = lbTileSetList.SelectedItem.Tag as TileSet;

            TileDisplay.SetTileSet(tileSet);
        }

        private void RefreshTileSets()
        {
            lbTileSetList.Clear();
            foreach (TileSet tileSet in theaterGraphics.Theater.TileSets)
            {
                if (tileSet.AllowToPlace && tileSet.LoadedTileCount > 0)
                    lbTileSetList.AddItem(new XNAListBoxItem() { Text = tileSet.SetName, Tag = tileSet });
            }
        }
    }
}
