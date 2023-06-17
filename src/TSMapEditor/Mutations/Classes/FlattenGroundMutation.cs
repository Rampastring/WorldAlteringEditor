using TSMapEditor.GameMath;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation for flattening ground. Especially useful when used with cliffs.
    /// Adjusts a target cell's height to a desired level and then processes all surrounding
    /// tiles for the height level to match.
    /// </summary>
    public class FlattenGroundMutation : AlterGroundElevationMutation
    {
        public FlattenGroundMutation(IMutationTarget mutationTarget, Point2D originCell, BrushSize brushSize, int desiredHeightLevel) : base(mutationTarget, originCell, brushSize)
        {
            this.desiredHeightLevel = desiredHeightLevel;
        }

        private readonly int desiredHeightLevel;

        public override void Perform() => FlattenGround();

        private void FlattenGround()
        {
            var cell = Map.GetTile(OriginCell);
            if (cell == null || cell.Level == desiredHeightLevel)
                return;

            AddCellToUndoData(OriginCell);
            cell.Level = (byte)desiredHeightLevel;
            cell.ChangeTileIndex(0, 0);
            foreach (Point2D surroundingCellOffset in SurroundingTiles)
            {
                RegisterCell(OriginCell + surroundingCellOffset);
            }

            MarkCellAsProcessed(OriginCell);

            Process();
        }
    }
}
