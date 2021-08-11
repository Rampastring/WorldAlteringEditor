using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class LocalVariable
    {
        public LocalVariable(int index)
        {
            Index = index;
        }

        public int Index { get; }
        public string Name { get; set; }
        public bool InitialState { get; set; }
    }
}
