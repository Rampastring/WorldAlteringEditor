using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Initialization;

namespace TSMapEditor.Models
{
    public class Rules
    {
        public List<UnitType> UnitTypes = new List<UnitType>();
        public List<InfantryType> InfantryTypes = new List<InfantryType>();
        public List<BuildingType> BuildingTypes = new List<BuildingType>();
        public List<AircraftType> AircraftTypes = new List<AircraftType>();
        public List<TerrainType> TerrainTypes = new List<TerrainType>();
        public List<OverlayType> OverlayTypes = new List<OverlayType>();

        /// <summary>
        /// Initializes rules types from an INI file.
        /// </summary>
        public void InitFromINI(IniFile iniFile, IInitializer initializer)
        {
            InitFromTypeSection(iniFile, "VehicleTypes", UnitTypes);
            InitFromTypeSection(iniFile, "InfantryTypes", InfantryTypes);
            InitFromTypeSection(iniFile, "BuildingTypes", BuildingTypes);
            InitFromTypeSection(iniFile, "AircraftTypes", AircraftTypes);
            InitFromTypeSection(iniFile, "TerrainTypes", TerrainTypes);
            InitFromTypeSection(iniFile, "OverlayTypes", OverlayTypes);

            // Go through all the lists and get object properties
            UnitTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
            InfantryTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
            BuildingTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
            AircraftTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
            TerrainTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
            OverlayTypes.ForEach(ut => initializer.ReadObjectTypePropertiesFromINI(ut, iniFile));
        }

        private void InitFromTypeSection<T>(IniFile iniFile, string sectionName, List<T> targetList)
        {
            var sectionKeys = iniFile.GetSectionKeys(sectionName);

            if (sectionKeys == null || sectionKeys.Count == 0)
                return;

            foreach (string key in sectionKeys)
            {
                string typeName = iniFile.GetStringValue(sectionName, key, null);

                // We assume that the type has a constructor
                // that takes a single string (ININame) as a parameter
                var constructor = typeof(T).GetConstructor(new Type[] { typeof(string) });
                if (constructor == null)
                {
                    throw new InvalidOperationException(typeof(T).FullName +
                        " has no public constructor that takes a single string as an argument!");
                }
                
                T objectInstance = (T)constructor.Invoke(new object[] { typeName });
                targetList.Add(objectInstance);
            }
        }
    }
}
