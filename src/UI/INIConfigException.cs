using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.UI
{
    /// <summary>
    /// The exception that is thrown when INI data is invalid.
    /// </summary>
    public class INIConfigException : Exception
    {
        public INIConfigException(string message) : base(message)
        {
        }
    }
}
