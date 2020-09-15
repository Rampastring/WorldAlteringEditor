using Rampastring.Tools;
using System;

namespace TSMapEditor.Models
{
    public abstract class AbstractObject
    {
        public abstract RTTIType WhatAmI();

        public void ReadPropertiesFromIniSection(IniSection iniSection)
        {
            var type = GetType();
            var propertyInfos = type.GetProperties();

            foreach (var property in propertyInfos)
            {
                var propertyType = property.PropertyType;

                if (propertyType.IsEnum)
                {
                    if (!property.CanWrite)
                        continue;

                    string value = iniSection.GetStringValue(property.Name, string.Empty);

                    if (string.IsNullOrEmpty(value))
                        continue;

                    property.SetValue(this, Enum.Parse(propertyType, value), null);
                    continue;
                }

                var setter = property.GetSetMethod();

                if (setter == null)
                    continue;

                if (propertyType.Equals(typeof(int)))
                    setter.Invoke(this, new object[] { iniSection.GetIntValue(property.Name, (int)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(double)))
                    setter.Invoke(this, new object[] { iniSection.GetDoubleValue(property.Name, (double)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(float)))
                    setter.Invoke(this, new object[] { iniSection.GetSingleValue(property.Name, (float)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(bool)))
                    setter.Invoke(this, new object[] { iniSection.GetBooleanValue(property.Name, (bool)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(string)))
                    setter.Invoke(this, new object[] { iniSection.GetStringValue(property.Name, (string)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(byte)))
                    setter.Invoke(this, new object[] { (byte)Math.Min(byte.MaxValue, iniSection.GetIntValue(property.Name, (byte)property.GetValue(this, null))) });
            }
        }
    }
}
