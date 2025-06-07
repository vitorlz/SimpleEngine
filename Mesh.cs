using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{
    public struct Mesh
    {
        public Mesh() { }
        public List<Vertex> Vertices { get; set; } = new List<Vertex>();
        public List<int> Indices { get; set; } = new List<int>();
    }
}
