using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;

namespace TSMapEditor.Initialization
{
    /// <summary>
    /// Initializes all different object types.
    /// </summary>
    public class Initializer : IInitializer
    {
        public Initializer(IMap map)
        {
            this.map = map;
        }

        private readonly IMap map;

        private Dictionary<Type, Action<AbstractObject, IniFile, IniSection>> objectTypeInitializers
            = new Dictionary<Type, Action<AbstractObject, IniFile, IniSection>>()
            {
                { typeof(BuildingType), InitBuildingType },
                { typeof(InfantryType), InitInfantryType },
                { typeof(UnitType), InitUnitType },
                { typeof(AircraftType), InitAircraftType },
                { typeof(OverlayType), InitOverlayType }
            };

        public void ReadObjectTypePropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(obj.ININame);
            if (objectSection == null)
                return;

            obj.ReadPropertiesFromIniSection(objectSection);

            if (objectTypeInitializers.TryGetValue(typeof(T), out Action<AbstractObject, IniFile, IniSection> action))
                action(obj, iniFile, objectSection);
        }

        public void InitArt(IniFile iniFile)
        {

        }

        private static void InitBuildingType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
            var buildingType = (BuildingType)obj;
        }

        private static void InitInfantryType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
        }

        private static void InitUnitType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
        }

        private static void InitAircraftType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
        }

        private static void InitOverlayType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
            var overlayType = (OverlayType)obj;
            overlayType.Land = (LandType)Enum.Parse(typeof(LandType), section.GetStringValue("Land", LandType.Clear.ToString()));
        }

        private static void InitTerrainType(AbstractObject obj, IniFile iniFile, IniSection section)
        {
            var terrainType = (TerrainType)obj;
            terrainType.SnowOccupationBits = (TerrainOccupation)section.GetIntValue("SnowOccupationBits", 0);
            terrainType.TemperateOccupationBits = (TerrainOccupation)section.GetIntValue("TemperateOccupationBits", 0);
        }
    }
}
