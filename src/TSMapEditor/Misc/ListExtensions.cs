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

        /// <summary>
        /// Fetches an element at the given index.
        /// If the element is out of bounds, returns null.
        /// </summary>
        public static T GetElementIfInRange<T>(this List<T> list, int index)
        {
            if (index < 0 || index >= list.Count)
                return default;

            return list[index];
        }
    }
}
