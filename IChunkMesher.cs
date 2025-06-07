using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{
    public interface IChunkMesher
    {
        Mesh GenerateMesh(Chunk chunk);
    }
}
