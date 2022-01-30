using Microsoft.Xna.Framework;
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
        private const int ResizeDragThreshold = 30;

        public TileSelector(WindowManager windowManager, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.theaterGraphics = theaterGraphics;
        }

        protected override void OnClientRectangleUpdated()
        {
            if (Initialized)
            {
                lbTileSetList.Height = Height;
                lbTileSetList.Width = TileSetListWidth;
                TileDisplay.Height = Height;
                TileDisplay.Width = Width - TileSetListWidth;
            }

            base.OnClientRectangleUpdated();
        }

        public TileDisplay TileDisplay { get; private set; }

        private TheaterGraphics theaterGraphics;
        private XNAListBox lbTileSetList;

        private bool isBeingDragged = false;
        private int previousMouseY;

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

            KeyboardCommands.Instance.NextTileSet.Action = NextTileSet;
            KeyboardCommands.Instance.PreviousTileSet.Action = PreviousTileSet;
        }

        private void NextTileSet()
        {
            if (lbTileSetList.Items.Count == 0)
                return;

            if (lbTileSetList.SelectedItem == null)
                lbTileSetList.SelectedIndex = 0;

            if (lbTileSetList.SelectedIndex == lbTileSetList.Items.Count - 1)
                return;

            lbTileSetList.SelectedIndex++;
        }

        private void PreviousTileSet()
        {
            if (lbTileSetList.Items.Count == 0)
                return;

            if (lbTileSetList.SelectedItem == null)
                lbTileSetList.SelectedIndex = lbTileSetList.Items.Count - 1;

            if (lbTileSetList.SelectedIndex == 0)
                return;

            lbTileSetList.SelectedIndex--;
        }

        public override void OnMouseLeftDown()
        {
            if (IsActive)
            {
                var cursorPoint = GetCursorPoint();

                if (!isBeingDragged && cursorPoint.Y > 0 && cursorPoint.Y < ResizeDragThreshold && Cursor.LeftDown)
                {
                    isBeingDragged = true;
                }
            }

            base.OnMouseLeftDown();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isBeingDragged)
            {
                var cursorPoint = GetCursorPoint();

                if (cursorPoint.Y < previousMouseY)
                {
                    int difference = previousMouseY - cursorPoint.Y;
                    Y -= difference;
                    Height += difference;

                    if (Height > WindowManager.RenderResolutionY)
                    {
                        Height = WindowManager.RenderResolutionY;
                        Y = 0;
                    }
                }
                else if (cursorPoint.Y > previousMouseY)
                {
                    int difference = cursorPoint.Y - previousMouseY;
                    Y += difference;
                    Height -= difference;

                    if (Height <= 10)
                    {
                        Height = 10;
                        Y = WindowManager.RenderResolutionY - ScaledHeight;
                    }
                }

                previousMouseY = GetCursorPoint().Y;

                if (!Cursor.LeftDown)
                    isBeingDragged = false;
            }
        }

        private void LbTileSetList_SelectedIndexChanged(object sender, EventArgs e)
        {
            TileSet tileSet = null;
            if (lbTileSetList.SelectedItem != null)
                tileSet = lbTileSetList.SelectedItem.Tag as TileSet;

            TileDisplay.SetTileSet(tileSet);

            // Unselect the listbox
            WindowManager.SelectedControl = null;
        }

        private void RefreshTileSets()
        {
            lbTileSetList.Clear();
            foreach (TileSet tileSet in theaterGraphics.Theater.TileSets)
            {
                if (tileSet.NonMarbleMadness > -1)
                    continue;

                if (tileSet.AllowToPlace && tileSet.LoadedTileCount > 0)
                    lbTileSetList.AddItem(new XNAListBoxItem() { Text = tileSet.SetName, Tag = tileSet });
            }
        }

        public override void Draw(GameTime gameTime)
        {
            FillRectangle(new Rectangle(0, 0, Width, ResizeDragThreshold), new Color(0, 0, 0, 64));
            DrawChildren(gameTime);
        }
    }
}
