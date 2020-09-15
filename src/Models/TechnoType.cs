using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public abstract class TechnoType : GameObjectType
    {
        public TechnoType(string iniName) : base(iniName)
        {
        }

        public string Image { get; set; }
    }
}
