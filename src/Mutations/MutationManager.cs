using System.Collections.Generic;

namespace TSMapEditor.Mutations
{
    /// <summary>
    /// The Undo / Redo system.
    /// </summary>
    public class MutationManager
    {
        public List<Mutation> UndoList { get; } = new List<Mutation>();
        public List<Mutation> RedoList { get; } = new List<Mutation>();

        /// <summary>
        /// Performs a new mutation on the map.
        /// </summary>
        /// <param name="mutation">The mutation to perform.</param>
        public void PerformMutation(Mutation mutation)
        {
            mutation.Perform();
            RedoList.Clear();
            UndoList.Add(mutation);
        }

        /// <summary>
        /// Undoes the last mutation to the map.
        /// </summary>
        public void Undo()
        {
            if (UndoList.Count == 0)
                return;

            int lastUndoIndex = UndoList.Count - 1;
            UndoList[lastUndoIndex].Undo();
            RedoList.Add(UndoList[lastUndoIndex]);
            UndoList.RemoveAt(lastUndoIndex);
        }

        /// <summary>
        /// Redoes the last un-done mutation on the map.
        /// </summary>
        public void Redo()
        {
            if (RedoList.Count == 0)
                return;

            int lastRedoIndex = RedoList.Count - 1;
            RedoList[lastRedoIndex].Perform();
            UndoList.Add(RedoList[lastRedoIndex]);
            RedoList.RemoveAt(lastRedoIndex);
        }
    }
}
