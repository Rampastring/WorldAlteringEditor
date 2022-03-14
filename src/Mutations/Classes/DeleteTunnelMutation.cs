using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    public class DeleteTubeMutation : Mutation
    {
        public DeleteTubeMutation(IMutationTarget mutationTarget, Point2D cellCoords) : base(mutationTarget)
        {
            this.cellCoords = cellCoords;
        }

        private readonly Point2D cellCoords;

        private Tube deletedTube;
        private int deletedTubeIndex;

        public override void Perform()
        {
            int index = MutationTarget.Map.Tubes.FindIndex(t => t.EntryPoint == cellCoords || t.ExitPoint == cellCoords);

            if (index > -1)
            {
                deletedTubeIndex = index;
                deletedTube = MutationTarget.Map.Tubes[index];
                MutationTarget.Map.Tubes.RemoveAt(index);

                RefreshTube(deletedTube);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(DeleteTubeMutation)}.{nameof(Perform)}: No tunnel found at tile {cellCoords}");
            }
        }

        public override void Undo()
        {
            MutationTarget.Map.Tubes.Insert(deletedTubeIndex, deletedTube);
            RefreshTube(deletedTube);
        }
        
        private void RefreshTube(Tube tube)
        {
            Point2D point = tube.EntryPoint;
            MutationTarget.AddRefreshPoint(point, 1);
            tube.Directions.ForEach(td =>
            {
                point = point.NextPointFromTubeDirection(td);

                int refreshSize = 0;

                switch (td)
                {
                    case TubeDirection.NorthEast:
                    case TubeDirection.NorthWest:
                    case TubeDirection.SouthEast:
                    case TubeDirection.SouthWest:
                        refreshSize = 0;
                        break;
                    case TubeDirection.North:
                    case TubeDirection.East:
                    case TubeDirection.South:
                    case TubeDirection.West:
                        refreshSize = 1;
                        break;
                }

                MutationTarget.AddRefreshPoint(point, refreshSize);
            });
            MutationTarget.AddRefreshPoint(tube.ExitPoint, 1);
        }
    }
}
