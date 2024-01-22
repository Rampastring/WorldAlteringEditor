using Rampastring.XNAUI.Input;
using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class PlaceTerrainCursorAction : CursorAction
    {
        public PlaceTerrainCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Place Terrain Tiles";

        public override bool HandlesKeyboardInput => true;

        private TileImage _tile;
        public TileImage Tile
        {
            get => _tile;
            set
            {
                _tile = value;
                heightOffset = 0;
            }
        }

        private int heightOffset;

        public override void OnActionEnter()
        {
            heightOffset = 0;
        }

        public override void OnKeyPressed(KeyPressEventArgs e)
        {
            if (Constants.IsFlatWorld)
                return;

            if (KeyboardCommands.Instance.AdjustTileHeightDown.Key.Key == e.PressedKey)
            {
                if (heightOffset > -Constants.MaxMapHeight)
                    heightOffset--;

                e.Handled = true;
            }
            else if (KeyboardCommands.Instance.AdjustTileHeightUp.Key.Key == e.PressedKey)
            {
                if (heightOffset < Constants.MaxMapHeight)
                    heightOffset++;

                e.Handled = true;
            }
        }

        private Point2D GetAdjustedCellCoords(Point2D cellCoords)
        {
            if (KeyboardCommands.Instance.PlaceTerrainBelow.AreKeysOrModifiersDown(CursorActionTarget.WindowManager.Keyboard))
                return cellCoords;

            // Don't place the tile where the user is pointing the cursor to,
            // but slightly above it - FinalSun also does this to not obstruct
            // the user's map view with the cursor
            int height = Tile.GetHeight();
            int cellHeight = (height / Constants.CellSizeY) - 1;

            Point2D newCellCoords = cellCoords - new Point2D(cellHeight, cellHeight);

            return newCellCoords;
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            // Assign preview data
            DoActionForCells(cellCoords, t => t.PreviewTileImage = Tile);
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            // Clear preview data
            DoActionForCells(cellCoords, t => t.PreviewTileImage = null);
        }

        private void DoActionForCells(Point2D cellCoords, Action<MapTile> action)
        {
            if (Tile == null)
                return;

            Point2D adjustedCellCoords = GetAdjustedCellCoords(cellCoords);

            MapTile originTile = CursorActionTarget.Map.GetTile(adjustedCellCoords);
            int originLevel = -1;

            BrushSize brush = CursorActionTarget.BrushSize;

            // First, look up the lowest point within the tile area for origin level
            // Only use a 1x1 brush size for this (meaning no brush at all)
            // so users can use larger brush sizes to "paint height"
            for (int i = 0; i < Tile.TMPImages.Length; i++)
            {
                MGTMPImage image = Tile.TMPImages[i];

                if (image.TmpImage == null)
                    continue;

                int cx = adjustedCellCoords.X + i % Tile.Width;
                int cy = adjustedCellCoords.Y + i / Tile.Width;

                var mapTile = MutationTarget.Map.GetTile(cx, cy);

                if (mapTile != null)
                {
                    var existingTile = Map.TheaterInstance.GetTile(mapTile.TileIndex).GetSubTile(mapTile.SubTileIndex);

                    int cellLevel = mapTile.Level;

                    // Allow replacing back cliffs
                    if (existingTile.TmpImage.Height == image.TmpImage.Height)
                        cellLevel -= existingTile.TmpImage.Height;

                    if (originLevel < 0 || cellLevel < originLevel)
                        originLevel = cellLevel;
                }
            }

            originLevel += heightOffset;
            if (originLevel < 0)
                originLevel = 0;

            // Then apply the preview data
            brush.DoForBrushSize(offset =>
            {
                for (int i = 0; i < Tile.TMPImages.Length; i++)
                {
                    MGTMPImage image = Tile.TMPImages[i];

                    if (image.TmpImage == null)
                        continue;

                    int cx = adjustedCellCoords.X + (offset.X * Tile.Width) + i % Tile.Width;
                    int cy = adjustedCellCoords.Y + (offset.Y * Tile.Height) + i / Tile.Width;

                    var mapTile = CursorActionTarget.Map.GetTile(cx, cy);
                    if (mapTile != null && (!CursorActionTarget.OnlyPaintOnClearGround || mapTile.IsClearGround()))
                    {
                        mapTile.PreviewSubTileIndex = i;
                        mapTile.PreviewLevel = Math.Min(originLevel + image.TmpImage.Height, Constants.MaxMapHeightLevel);
                        action(mapTile);
                    }
                }
            });

            CursorActionTarget.AddRefreshPoint(adjustedCellCoords, Math.Max(Tile.Width, Tile.Height) * Math.Max(brush.Width, brush.Height) + 1);
        }

        public override void LeftDown(Point2D cellCoords)
        {
            if (Tile == null)
                return;

            Point2D adjustedCellCoords = GetAdjustedCellCoords(cellCoords);

            Mutation mutation = null;

            if (KeyboardCommands.Instance.FillTerrain.AreKeysOrModifiersDown(CursorActionTarget.WindowManager.Keyboard)
                && (Tile.Width == 1 && Tile.Height == 1))
            {
                var targetCell = CursorActionTarget.Map.GetTile(adjustedCellCoords);

                if (targetCell != null)
                {
                    mutation = new FillTerrainAreaMutation(CursorActionTarget.MutationTarget, targetCell, Tile);
                }
            }
            else
            {
                mutation = new PlaceTerrainTileMutation(CursorActionTarget.MutationTarget, adjustedCellCoords, Tile, heightOffset);
            }

            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            LeftDown(cellCoords);
        }
    }
}
