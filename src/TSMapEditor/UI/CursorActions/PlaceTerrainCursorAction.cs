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

        public TileImage Tile { get; set; }

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
            int originLevel = originTile?.Level ?? -1;

            BrushSize brush = CursorActionTarget.BrushSize;

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
                        if (originLevel < 0)
                            originLevel = mapTile.Level;

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
                mutation = new PlaceTerrainTileMutation(CursorActionTarget.MutationTarget, adjustedCellCoords, Tile);
            }

            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            LeftDown(cellCoords);
        }
    }
}
