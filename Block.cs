using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{

    public class Block
    {
        public enum Type
        {
            GRASS,
            WATER,
            SNOW
        }

        public Type type = Type.GRASS;
        public bool Active { get; set; } = false;
        
    }
}
