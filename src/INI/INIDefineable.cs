using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.INI
{
    public class INIDefineable
    {
        public void ParseAttributes()
        {

        }

        protected void SetPropertiesFromSection(IniSection section)
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

                    string value = section.GetStringValue(property.Name, string.Empty);

                    if (string.IsNullOrEmpty(value))
                        continue;

                    property.SetValue(this, Enum.Parse(propertyType, value), null);
                    continue;
                }

                var setter = property.GetSetMethod(true);

                if (setter == null)
                    continue;

                if (propertyType.Equals(typeof(int)))
                    setter.Invoke(this, new object[] { section.GetIntValue(property.Name, (int)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(double)))
                    setter.Invoke(this, new object[] { section.GetDoubleValue(property.Name, (double)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(float)))
                    setter.Invoke(this, new object[] { section.GetSingleValue(property.Name, (float)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(bool)))
                    setter.Invoke(this, new object[] { section.GetBooleanValue(property.Name, (bool)property.GetValue(this, null)) });
                else if (propertyType.Equals(typeof(string)))
                    setter.Invoke(this, new object[] { section.GetStringValue(property.Name, (string)property.GetValue(this, null)) });
            }
        }
    }
}
