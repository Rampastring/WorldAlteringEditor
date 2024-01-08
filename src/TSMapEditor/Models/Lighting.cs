using Rampastring.Tools;

namespace TSMapEditor.Models
{
    public class Lighting : INIDefineable
    {
        private const string LightingIniSectionName = "Lighting";

        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
        public double Ambient { get; set; }
        public double Level { get; set; }
        public double Ground { get; set; }
        
        public double IonRed { get; set; }
        public double IonGreen { get; set; }
        public double IonBlue { get; set; }
        public double IonAmbient { get; set; }
        public double IonLevel { get; set; }
        public double IonGround { get; set; }

        public double? DominatorRed { get; set; }
        public double? DominatorGreen { get; set; }
        public double? DominatorBlue { get; set; }
        public double? DominatorAmbient { get; set; }
        public double? DominatorAmbientChangeRate { get; set; }
        public double? DominatorLevel { get; set; }
        public double? DominatorGround { get; set; }

        public void ReadFromIniFile(IniFile iniFile)
        {
            var lightingSection = iniFile.GetSection(LightingIniSectionName);

            if (lightingSection == null)
                return;

            ReadPropertiesFromIniSection(lightingSection);
        }

        public void WriteToIniFile(IniFile iniFile)
        {
            var lightingSection = iniFile.GetSection(LightingIniSectionName);
            if (lightingSection == null)
            {
                lightingSection = new IniSection(LightingIniSectionName);
                iniFile.AddSection(lightingSection);
            }

            WritePropertiesToIniSection(lightingSection);
        }
    }
}
