using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace SimpleEngine.Voxels
{
    public class Quad
    {
        public Vector3 Start { get; set; }
        public Vector3 DirU { get; set; }
        public Vector3 DirV { get; set; }
        public Vector3 Normal { get; set; }
        public int SizeU { get; set; } = 1;
        public int SizeV { get; set; } = 1;

        public Quad(Vector3 start, Vector3 dirU, Vector3 dirV, Vector3 normal)
        {
            Start = start;
            DirU = dirU;
            DirV = dirV;
            Normal = normal;
        }
    }
}
