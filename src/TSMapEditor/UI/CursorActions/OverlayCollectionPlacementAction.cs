using System;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class OverlayCollectionPlacementAction : CursorAction
    {
        public OverlayCollectionPlacementAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Place Overlay Collection";

        public OverlayCollection OverlayCollection { get; set; }

        struct OriginalOverlayInfo
        {
            public OverlayType OverlayType;
            public int FrameIndex;

            public OriginalOverlayInfo(OverlayType overlayType, int frameIndex)
            {
                OverlayType = overlayType;
                FrameIndex = frameIndex;
            }
        }

        private List<OriginalOverlayInfo> originalOverlay = new List<OriginalOverlayInfo>();
        private int[] randomizedOverlayTypeIndexes;
        private OverlayCollection lastRandomizedCollection = null;

        public override void PreMapDraw(Point2D cellCoords)
        {
            originalOverlay.Clear();

            int brushSize = CursorActionTarget.BrushSize.Width * CursorActionTarget.BrushSize.Height;
            if (lastRandomizedCollection != OverlayCollection || 
                randomizedOverlayTypeIndexes.Length < brushSize)
            {
                randomizedOverlayTypeIndexes = new int[brushSize];
                for (int i = 0; i < randomizedOverlayTypeIndexes.Length; i++)
                    randomizedOverlayTypeIndexes[i] = CursorActionTarget.Randomizer.GetRandomNumber(0, OverlayCollection.OverlayTypes.Length - 1);

                lastRandomizedCollection = OverlayCollection;
            }

            int tileIndex = 0;
            CursorActionTarget.BrushSize.DoForBrushSize(offset =>
            {
                var tile = CursorActionTarget.Map.GetTile(cellCoords + offset);
                if (tile == null)
                    return;

                // Store original overlay info
                if (tile.Overlay != null)
                    originalOverlay.Add(new OriginalOverlayInfo(tile.Overlay.OverlayType, tile.Overlay.FrameIndex));
                else
                    originalOverlay.Add(new OriginalOverlayInfo(null, Constants.NO_OVERLAY));

                OverlayType overlayType = OverlayCollection.OverlayTypes[randomizedOverlayTypeIndexes[tileIndex]];

                // Do not place tiberium on impassable tiles
                if (overlayType.Tiberium)
                {
                    ITileImage tileImage = CursorActionTarget.Map.TheaterInstance.GetTile(tile.TileIndex);
                    ISubTileImage subCellImage = tileImage.GetSubTile(tile.SubTileIndex);
                    if (Helpers.IsLandTypeImpassable(subCellImage.TmpImage.TerrainType))
                    {
                        tileIndex++;
                        return;
                    }
                }

                // Apply new overlay info
                if (tile.Overlay == null)
                {
                    // Creating new object instances each frame is not very performance-friendly, we might want to revise this later...
                    tile.Overlay = new Overlay()
                    {
                        Position = tile.CoordsToPoint(),
                        OverlayType = overlayType
                    };
                }
                else
                {
                    tile.Overlay.OverlayType = overlayType;
                }

                tile.Overlay.FrameIndex = CursorActionTarget.Map.GetOverlayFrameIndex(tile.CoordsToPoint());

                tileIndex++;
            });

            CursorActionTarget.AddRefreshPoint(cellCoords, Math.Max(CursorActionTarget.BrushSize.Height, CursorActionTarget.BrushSize.Width));
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            int index = 0;

            CursorActionTarget.BrushSize.DoForBrushSize(offset =>
            {
                var tile = CursorActionTarget.Map.GetTile(cellCoords + offset);
                if (tile == null)
                    return;

                if (OverlayCollection.OverlayTypes[0].Tiberium)
                {
                    ITileImage tileImage = CursorActionTarget.Map.TheaterInstance.GetTile(tile.TileIndex);
                    ISubTileImage subCellImage = tileImage.GetSubTile(tile.SubTileIndex);
                    if (Helpers.IsLandTypeImpassable(subCellImage.TmpImage.TerrainType))
                    {
                        index++;
                        return;
                    }
                }

                var originalOverlayData = originalOverlay[index];

                if (originalOverlayData.OverlayType == null)
                {
                    tile.Overlay = null;
                }
                else
                {
                    tile.Overlay.OverlayType = originalOverlayData.OverlayType;
                    tile.Overlay.FrameIndex = originalOverlayData.FrameIndex;
                }

                index++;
            });

            originalOverlay.Clear();

            CursorActionTarget.AddRefreshPoint(cellCoords, Math.Max(CursorActionTarget.BrushSize.Height, CursorActionTarget.BrushSize.Width));
        }

        public override void LeftDown(Point2D cellCoords)
        {
            var mutation = new PlaceOverlayCollectionMutation(CursorActionTarget.MutationTarget, OverlayCollection, cellCoords);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellCoords) => LeftDown(cellCoords);
    }
}
