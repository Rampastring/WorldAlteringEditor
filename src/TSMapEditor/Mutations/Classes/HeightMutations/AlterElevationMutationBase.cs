using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes.HeightMutations
{
    public abstract class AlterElevationMutationBase : Mutation
    {
        protected AlterElevationMutationBase(IMutationTarget mutationTarget) : base(mutationTarget)
        {
        }

        protected bool IsCellMorphable(MapTile cell)
        {
            return Map.TheaterInstance.Theater.TileSets[Map.TheaterInstance.GetTileSetId(cell.TileIndex)].Morphable;
        }
    }
}
