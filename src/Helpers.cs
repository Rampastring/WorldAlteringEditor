using System;

namespace TSMapEditor
{
    public static class Helpers
    {
        public static bool IsStringNoneValue(string str)
        {
            return str.Equals(Constants.NoneValue1, StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals(Constants.NoneValue2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string BoolToIntString(bool value)
        {
            return value ? "1" : "0";
        }
    }
}
