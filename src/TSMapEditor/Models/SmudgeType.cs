using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class SmudgeType : GameObjectType
    {
        public SmudgeType(string iniName) : base(iniName)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Smudge;


        /// <summary>
        /// Defined in Art.ini. If set to true,
        /// the art for this smudge type is theater-specific;
        /// if false, the art is a generic .SHP used for every theater.
        /// </summary>
        public bool Theater { get; set; }

        public bool Crater { get; set; }
        public bool Burn { get; set; }
    }
}
