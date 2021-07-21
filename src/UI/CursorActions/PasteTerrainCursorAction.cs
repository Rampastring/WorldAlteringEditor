using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows pasting previously copied terrain.
    /// </summary>
    public class PasteTerrainCursorAction : CursorAction
    {
        public PasteTerrainCursorAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            int maxOffset = 0;

            foreach (var copiedTerrain in CursorActionTarget.CopiedTerrainData)
            {
                maxOffset = Math.Max(maxOffset, Math.Max(Math.Abs(copiedTerrain.Offset.X), Math.Abs(copiedTerrain.Offset.Y)));

                MapTile cell = CursorActionTarget.Map.GetTile(cellCoords + copiedTerrain.Offset);
                if (cell == null)
                    continue;

                cell.PreviewTileImage = CursorActionTarget.TheaterGraphics.GetTileGraphics(copiedTerrain.TileIndex, 0);
                cell.PreviewSubTileIndex = copiedTerrain.SubTileIndex;
                
            }

            CursorActionTarget.AddRefreshPoint(cellCoords, maxOffset);
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            int maxOffset = 0;

            foreach (var copiedTerrain in CursorActionTarget.CopiedTerrainData)
            {
                maxOffset = Math.Max(maxOffset, Math.Max(Math.Abs(copiedTerrain.Offset.X), Math.Abs(copiedTerrain.Offset.Y)));

                MapTile cell = CursorActionTarget.Map.GetTile(cellCoords + copiedTerrain.Offset);
                if (cell == null)
                    continue;

                cell.PreviewTileImage = null;
            }

            CursorActionTarget.AddRefreshPoint(cellCoords, maxOffset);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            if (CursorActionTarget.Map.GetTile(cellCoords) == null)
                return;

            var mutation = new PasteTerrainMutation(CursorActionTarget.MutationTarget, CursorActionTarget.CopiedTerrainData, cellCoords);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }
    }
}
