using Rampastring.Tools;
using System;

namespace TSMapEditor.Models
{
    public abstract class AbstractObject : INIDefineable
    {
        public abstract RTTIType WhatAmI();
    }
}
