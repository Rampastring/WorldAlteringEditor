using System.Collections.Generic;

namespace TSMapEditor.Misc
{
    public static class ListExtensions
    {
        /// <summary>
        /// Swaps two list items.
        /// </summary>
        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            (list[index1], list[index2]) = (list[index2], list[index1]);
        }
    }
}
