using System.Globalization;

namespace TSMapEditor.Models
{
    public class TriggerAction
    {
        public const int PARAM_COUNT = 7;
        public const int INI_VALUE_COUNT = PARAM_COUNT + 1;

        public int ActionIndex { get; set; }
        public string[] Parameters { get; } = new string[7];

        public string ParamToString(int index)
        {
            if (Parameters[index] == null)
                return "0";

            return Parameters[index];
        }

        public static TriggerAction ParseFromArray(string[] array, int startIndex)
        {
            if (startIndex + INI_VALUE_COUNT > array.Length)
                return null;

            int actionIndex = int.Parse(array[startIndex], CultureInfo.InvariantCulture);

            var triggerAction = new TriggerAction();
            triggerAction.ActionIndex = actionIndex;
            for (int i = 0; i < PARAM_COUNT; i++)
                triggerAction.Parameters[i] = array[startIndex + 1 + i];

            return triggerAction;
        }
    }
}
