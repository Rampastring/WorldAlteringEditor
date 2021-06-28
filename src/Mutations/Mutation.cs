namespace TSMapEditor.Mutations
{
    /// <summary>
    /// A base class for all mutations.
    /// A mutation modifies something in the map in a way that makes the effect
    /// un-doable and re-doable through the Undo/Redo system.
    /// </summary>
    public abstract class Mutation
    {
        public abstract void Perform();

        public abstract void Undo();
    }
}
