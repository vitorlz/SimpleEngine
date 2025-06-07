using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace SimpleEngine.Voxels
{
    public struct Vertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;
        public int type;
    }
}
