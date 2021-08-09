using Rampastring.Tools;
using System;
using System.Collections.Generic;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A map trigger.
    /// </summary>
    public class Trigger
    {
        public Trigger(string id) { ID = id; }

        public string ID { get; private set; }
        public string House { get; set; }

        /// <summary>
        /// The linked trigger ID loaded from the map.
        /// Do not use post map loading.
        /// </summary>
        public string LinkedTriggerId { get; set; } = Constants.NoneValue2;
        public Trigger LinkedTrigger { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public bool Easy { get; set; } = true;
        public bool Normal { get; set; } = true;
        public bool Hard { get; set; } = true;

        public List<TriggerCondition> Conditions { get; private set; } = new List<TriggerCondition>();
        public List<TriggerAction> Actions { get; private set; } = new List<TriggerAction>();

        /// <summary>
        /// Creates and returns a deep clone of this trigger.
        /// </summary>
        /// <param name="uniqueId">The unique ID of the new trigger.</param>
        public Trigger Clone(string uniqueId)
        {
            Trigger clone = (Trigger)MemberwiseClone();
            clone.ID = uniqueId;
            clone.Name = "Clone of " + Name;

            // Deep clone the events and actions

            clone.Conditions = new List<TriggerCondition>(Conditions.Capacity);
            foreach (var condition in Conditions)
            {
                clone.Conditions.Add(condition.Clone());
            }

            clone.Actions = new List<TriggerAction>(Actions.Capacity);
            foreach (var action in Actions)
            {
                clone.Actions.Add(action.Clone());
            }

            return clone;
        }

        public void WriteToIniFile(IniFile iniFile)
        {
            // Write entry to [Triggers]
            string linkedTriggerId = LinkedTrigger == null ? Constants.NoneValue1 : LinkedTrigger.ID;
            iniFile.SetStringValue("Triggers", ID,
                $"{House},{linkedTriggerId},{Name}," +
                $"{Helpers.BoolToIntString(Disabled)}," +
                $"{Helpers.BoolToIntString(Easy)},{Helpers.BoolToIntString(Normal)},{Helpers.BoolToIntString(Hard)},0");

            // Write entry to [Events]
            var conditionDataString = new ExtendedStringBuilder(true, ',');
            conditionDataString.Append(Conditions.Count);
            foreach (var condition in Conditions)
            {
                conditionDataString.Append(condition.ConditionIndex);
                conditionDataString.Append(condition.Parameter1);
                conditionDataString.Append(condition.Parameter2);
            }

            iniFile.SetStringValue("Events", ID, conditionDataString.ToString());

            // Write entry to [Actions]
            var actionDataString = new ExtendedStringBuilder(true, ',');
            actionDataString.Append(Actions.Count);
            foreach (var action in Actions)
            {
                actionDataString.Append(action.ActionIndex);
                for (int i = 0; i < TriggerAction.PARAM_COUNT; i++)
                    actionDataString.Append(action.ParamToString(i));
            }

            iniFile.SetStringValue("Actions", ID, actionDataString.ToString());
        }

        public void ParseConditions(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] dataArray = data.Split(',');

            int eventCount = Conversions.IntFromString(dataArray[0], -1);
            if (eventCount < 0)
                return;

            int startIndex = 1;
            for (int i = 0; i < eventCount; i++)
            {
                var triggerEvent = TriggerCondition.ParseFromArray(dataArray, startIndex);
                if (triggerEvent == null)
                    return;

                startIndex += TriggerCondition.INI_VALUE_COUNT;
                Conditions.Add(triggerEvent);
            }
        }

        public void ParseActions(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            string[] dataArray = data.Split(',');

            int actionCount = Conversions.IntFromString(dataArray[0], -1);
            if (actionCount < 0)
                return;

            int startIndex = 1;
            for (int i = 0; i < actionCount; i++)
            {
                var triggerAction = TriggerAction.ParseFromArray(dataArray, startIndex);
                if (triggerAction == null)
                    return;

                startIndex += TriggerAction.INI_VALUE_COUNT;
                Actions.Add(triggerAction);
            }
        }

        /// <summary>
        /// Parses and creates a trigger instance from a trigger data line
        /// in a Tiberian Sun / Red Alert 2 map file.
        /// </summary>
        /// <param name="id">The ID of the trigger.</param>
        /// <param name="data">The data line.</param>
        /// <returns>A Trigger instance if the parsing succeeds, otherwise null.</returns>
        public static Trigger ParseTrigger(string id, string data)
        {
            // [Triggers]
            // ID=HOUSE,LINKED_TRIGGER,NAME,DISABLED,EASY,NORMAL,HARD,REPEATING
            // https://modenc.renegadeprojects.com/Triggers
            // the 'REPEATING' field here is unused by the game, so we ignore it

            string[] parts = data.Split(',');
            if (parts.Length < 7)
                return null;

            return new Trigger(id)
            {
                House = parts[0],
                LinkedTriggerId = parts[1],
                Name = parts[2],
                Disabled = Conversions.BooleanFromString(parts[3], false),
                Easy = Conversions.BooleanFromString(parts[4], true),
                Normal = Conversions.BooleanFromString(parts[5], true),
                Hard = Conversions.BooleanFromString(parts[6], true),
            };
        }
    }
}
