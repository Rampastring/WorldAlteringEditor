using Rampastring.Tools;
using System;
using System.Collections.Generic;
using TSMapEditor.Initialization;
using TSMapEditor.Models.ArtConfig;

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

        public List<InfantrySequence> InfantrySequences = new List<InfantrySequence>();
        public List<RulesColor> Colors = new List<RulesColor>();

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

            var colorsSection = iniFile.GetSection("Colors");
            if (colorsSection != null)
            {
                foreach (var kvp in colorsSection.Keys)
                {
                    Colors.Add(new RulesColor(kvp.Key, kvp.Value));
                }
            }
        }

        public void InitArt(IniFile iniFile, IInitializer initializer)
        {
            TerrainTypes.ForEach(tt => initializer.ReadObjectTypeArtPropertiesFromINI(tt, iniFile));

            BuildingTypes.ForEach(bt => initializer.ReadObjectTypeArtPropertiesFromINI(bt, iniFile,
                string.IsNullOrWhiteSpace(bt.Image) ? bt.ININame : bt.Image));

            UnitTypes.ForEach(ut => initializer.ReadObjectTypeArtPropertiesFromINI(ut, iniFile,
                string.IsNullOrWhiteSpace(ut.Image) ? ut.ININame : ut.Image));

            InfantryTypes.ForEach(it => initializer.ReadObjectTypeArtPropertiesFromINI(it, iniFile,
                string.IsNullOrWhiteSpace(it.Image) ? it.ININame : it.Image));

            OverlayTypes.ForEach(ot => initializer.ReadObjectTypeArtPropertiesFromINI(ot, iniFile,
                string.IsNullOrWhiteSpace(ot.Image) ? ot.ININame : ot.Image));
        }

        public List<House> GetStandardHouses(IniFile iniFile)
        {
            var housesSection = iniFile.GetSection("Houses");
            if (housesSection == null)
                return new List<House>(0);

            var houses = new List<House>();

            foreach (var kvp in housesSection.Keys)
            {
                string houseName = kvp.Value;
                var house = new House(houseName);
                InitHouse(iniFile, house);
                houses.Add(house);
            }

            return houses;
        }

        private void InitHouse(IniFile iniFile, House house)
        {
            house.Name = iniFile.GetStringValue(house.ININame, "Name", house.ININame);
            house.Color = iniFile.GetStringValue(house.ININame, "Color", "Grey");
        }

        private void InitFromTypeSection<T>(IniFile iniFile, string sectionName, List<T> targetList)
        {
            var sectionKeys = iniFile.GetSectionKeys(sectionName);

            if (sectionKeys == null || sectionKeys.Count == 0)
                return;

            int i = 0;

            foreach (string key in sectionKeys)
            {
                string typeName = iniFile.GetStringValue(sectionName, key, null);

                var objectType = typeof(T);

                // We assume that the type has a constructor
                // that takes a single string (ININame) as a parameter
                var constructor = objectType.GetConstructor(new Type[] { typeof(string) });
                if (constructor == null)
                {
                    throw new InvalidOperationException(typeof(T).FullName +
                        " has no public constructor that takes a single string as an argument!");
                }

                T objectInstance = (T)constructor.Invoke(new object[] { typeName });

                // Set the index property if one exists
                var indexProperty = objectType.GetProperty("Index");
                if (indexProperty != null)
                    indexProperty.SetValue(objectInstance, i);

                targetList.Add(objectInstance);
                i++;
            }
        }

        public InfantrySequence FindOrMakeInfantrySequence(IniFile artIni, string infantrySequenceName)
        {
            var existing = InfantrySequences.Find(seq => seq.ININame == infantrySequenceName);
            if (existing == null)
            {
                existing = new InfantrySequence(infantrySequenceName);
                var section = artIni.GetSection(infantrySequenceName);
                if (section == null)
                    throw new KeyNotFoundException("Infantry sequence not found: " + infantrySequenceName);

                existing.ParseFromINISection(section);
                InfantrySequences.Add(existing);
            }

            return existing;
        }
    }
}
