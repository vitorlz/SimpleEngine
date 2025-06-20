﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{
    public struct Block
    {
        public Block() { }
        public enum Type
        {
            GRASS = 0,
            WATER = 1,
            SNOW = 2,
            STONE = 3,
            WOOD = 4,
            LEAVES = 5,
        }
        public Type type = Type.GRASS;
        public bool Active { get; set; } = false;
    }
}
