using Rampastring.Tools;
using System;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A trigger condition ("Event").
    /// </summary>
    public class TriggerCondition : ICloneable
    {
        public const int INI_VALUE_COUNT = 3;

        public int ConditionIndex { get; set; }
        public int Parameter1 { get; set; }
        public int Parameter2 { get; set; }

        public object Clone() => DoClone();

        public TriggerCondition DoClone()
        {
            return (TriggerCondition)MemberwiseClone();
        }

        public static TriggerCondition ParseFromArray(string[] array, int startIndex)
        {
            if (startIndex + INI_VALUE_COUNT > array.Length)
                return null;

            var triggerCondition = new TriggerCondition();
            triggerCondition.ConditionIndex = Conversions.IntFromString(array[startIndex], -1);
            triggerCondition.Parameter1 = Conversions.IntFromString(array[startIndex + 1], -1);
            triggerCondition.Parameter2 = Conversions.IntFromString(array[startIndex + 2], -1);

            if (triggerCondition.ConditionIndex < 0)
                return null;

            return triggerCondition;
        }
    }
}
