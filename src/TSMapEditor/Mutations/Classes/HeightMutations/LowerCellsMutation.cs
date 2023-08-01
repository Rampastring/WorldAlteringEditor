using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Rendering;
using TSMapEditor.UI;

namespace TSMapEditor.Mutations.Classes.HeightMutations
{
    /// <summary>
    /// A mutation for lowering cell height levels.
    /// </summary>
    public class LowerCellsMutation : Mutation
    {
        public LowerCellsMutation(IMutationTarget mutationTarget, Point2D targetCellCoords, BrushSize brushSize) : base(mutationTarget)
        {
            this.targetCellCoords = targetCellCoords;
            this.brushSize = brushSize;
        }

        private Point2D targetCellCoords;
        private BrushSize brushSize;

        private List<Point2D> affectedCells = new List<Point2D>();

        public override void Perform()
        {
            brushSize.DoForBrushSize(offset =>
            {
                Point2D cellCoords = targetCellCoords + offset;

                var cell = MutationTarget.Map.GetTile(cellCoords);
                if (cell == null)
                    return;

                if (cell.Level > 0)
                {
                    cell.Level--;
                    affectedCells.Add(cellCoords);
                }
            });

            MutationTarget.AddRefreshPoint(targetCellCoords);
        }

        public override void Undo()
        {
            foreach (Point2D cellCoords in affectedCells)
            {
                var cell = MutationTarget.Map.GetTile(cellCoords);
                if (cell == null)
                    return;

                if (cell.Level < Constants.MaxMapHeight)
                    cell.Level++;
            }

            MutationTarget.AddRefreshPoint(targetCellCoords);
        }
    }
}
