using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models;

namespace TSMapEditor.Initialization
{
    public interface IInitializer
    {
        void ReadObjectTypePropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined;
    }
}
