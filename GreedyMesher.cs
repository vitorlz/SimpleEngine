using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace SimpleEngine.Voxels
{
    public class GreedyMesher : IChunkMesher
    {
        private Mesh _mesh = new Mesh();
        public Mesh GenerateMesh(Chunk chunk)
        {
            greedyNX(chunk);
            greedyPX(chunk);
            greedyPY(chunk);
            greedyNY(chunk);
            greedyNZ(chunk);
            greedyPZ(chunk);
            return _mesh;
        }

        private void greedyNX(Chunk chunk)
        {
            Vector3 dirU = new Vector3(0f, 0f, 1f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(-1f, 0f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int x = 0; x < chunk.ChunkSize; x++)
            {
                bool[,] uncovered = new bool[chunk.ChunkHeight, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkHeight, chunk.ChunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int z = 0; z < chunk.ChunkSize; z++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (x == 0 || !chunk.Blocks[x - 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int z = 0; z < chunk.ChunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }

                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y, z), dirU, dirV, norm);
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (y + vOffset < chunk.ChunkHeight && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z] && chunk.Blocks[x, y + vOffset, z].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (z + uOffset < chunk.ChunkSize && y + i < chunk.ChunkHeight && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset] && chunk.Blocks[x, y + i, z + uOffset].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[y + i, z + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }


        private void greedyPX(Chunk chunk)
        {
            Vector3 dirU = new Vector3(0f, 0f, 1f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(1f, 0f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int x = 0; x < chunk.ChunkSize; x++)
            {
                bool[,] uncovered = new bool[chunk.ChunkHeight, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkHeight, chunk.ChunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int z = 0; z < chunk.ChunkSize; z++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (x == chunk.ChunkSize - 1 || !chunk.Blocks[x + 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int z = 0; z < chunk.ChunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // create a quad
                        Quad quad = new Quad(new Vector3(x + 1, y, z), dirU, dirV, norm);

                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (y + vOffset < chunk.ChunkHeight && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z] && chunk.Blocks[x, y + vOffset, z].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (z + uOffset < chunk.ChunkSize && y + i < chunk.ChunkHeight && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset] && chunk.Blocks[x, y + i, z + uOffset].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[y + i, z + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }

        private void greedyPY(Chunk chunk)
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 0f, 1f);
            Vector3 norm = new Vector3(0f, 1f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int y = 0; y < chunk.ChunkHeight; y++)
            {
                bool[,] uncovered = new bool[chunk.ChunkSize, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkSize, chunk.ChunkSize];

                // check which faces are uncovered
                for (int z = 0; z < chunk.ChunkSize; z++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (y == chunk.ChunkHeight - 1 || !chunk.Blocks[x, y + 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < chunk.ChunkSize; z++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y + 1, z), dirU, dirV, norm);

                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (z + vOffset < chunk.ChunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x] && chunk.Blocks[x, y, z + vOffset].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (x + uOffset < chunk.ChunkSize && z + i < chunk.ChunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset] && chunk.Blocks[x + uOffset, y, z + i].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[z + i, x + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }


        private void greedyNY(Chunk chunk)
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 0f, 1f);
            Vector3 norm = new Vector3(0f, -1f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int y = 0; y < chunk.ChunkHeight; y++)
            {
                bool[,] uncovered = new bool[chunk.ChunkSize, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkSize, chunk.ChunkSize];

                // check which faces are uncovered
                for (int z = 0; z < chunk.ChunkSize; z++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (y == 0 || !chunk.Blocks[x, y - 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < chunk.ChunkSize; z++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y, z), dirU, dirV, norm);

                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (z + vOffset < chunk.ChunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x] && chunk.Blocks[x, y, z + vOffset].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (x + uOffset < chunk.ChunkSize && z + i < chunk.ChunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset] && chunk.Blocks[x + uOffset, y, z + i].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[z + i, x + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }

        private void greedyNZ(Chunk chunk)
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(0f, 0f, -1f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                bool[,] uncovered = new bool[chunk.ChunkHeight, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkHeight, chunk.ChunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (z == 0 || !chunk.Blocks[x, y, z - 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y, z), dirU, dirV, norm);

                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (y + vOffset < chunk.ChunkHeight && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x] && chunk.Blocks[x, y + vOffset, z].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (x + uOffset < chunk.ChunkSize && y + i < chunk.ChunkHeight && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset] && chunk.Blocks[x + uOffset, y + i, z].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[y + i, x + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }

        public void greedyPZ(Chunk chunk)
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(0f, 0f, 1f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int z = 0; z < chunk.ChunkSize; z++)
            {
                bool[,] uncovered = new bool[chunk.ChunkHeight, chunk.ChunkSize];
                bool[,] inAQuad = new bool[chunk.ChunkHeight, chunk.ChunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        if (chunk.Blocks[x, y, z].Active && (z == chunk.ChunkSize - 1 || !chunk.Blocks[x, y, z + 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < chunk.ChunkHeight; y++)
                {
                    for (int x = 0; x < chunk.ChunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }
                        Block.Type neededType = chunk.Blocks[x, y, z].type;
                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y, z + 1), dirU, dirV, norm);

                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (y + vOffset < chunk.ChunkHeight && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x] && chunk.Blocks[x, y + vOffset, z].type == neededType)
                            {
                                vOffset++;
                                continue;
                            }

                            expandV = false;
                        }

                        // try to expand sideways. Same thing as before, but we have to check if there is a section to the side
                        // of the same type and height (vOffset) as the current height.
                        while (expandU)
                        {
                            for (int i = 0; i < vOffset; i++)
                            {
                                if (x + uOffset < chunk.ChunkSize && y + i < chunk.ChunkHeight && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset] && chunk.Blocks[x + uOffset, y + i, z].type == neededType)
                                {
                                    continue;
                                }

                                expandU = false;
                                break;
                            }

                            if (expandU)
                            {
                                uOffset++;
                            }
                        }

                        for (int i = 0; i < vOffset; i++)
                        {
                            for (int j = 0; j < uOffset; j++)
                            {
                                inAQuad[y + i, x + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;

                        EmitQuad(chunk, quad, (int)chunk.Blocks[x, y, z].type);
                    }
                }
            }
        }

        private void EmitQuad(Chunk chunk, Quad quad, int type)
        {
            Vector3 offset = new Vector3(chunk.Pos.X, 0, chunk.Pos.Y);

            Vector3 pos0 = quad.Start;
            Vector3 pos1 = quad.Start + quad.DirU * quad.SizeU;
            Vector3 pos2 = quad.Start + quad.DirU * quad.SizeU + quad.DirV * quad.SizeV;
            Vector3 pos3 = quad.Start + quad.DirV * quad.SizeV;

            pos0 += offset;
            pos1 += offset;
            pos2 += offset;
            pos3 += offset;

            Vertex v0 = new Vertex() { pos = pos0, normal = quad.Normal, uv = new Vector2(0.0f, 0.0f), type = type };
            Vertex v1 = new Vertex() { pos = pos1, normal = quad.Normal, uv = new Vector2(quad.SizeU, 0.0f), type = type };
            Vertex v2 = new Vertex() { pos = pos2, normal = quad.Normal, uv = new Vector2(quad.SizeU, quad.SizeV), type = type };
            Vertex v3 = new Vertex() { pos = pos3, normal = quad.Normal, uv = new Vector2(0.0f, quad.SizeV), type = type };

            int baseIndex = _mesh.Vertices.Count;
            _mesh.Vertices.Add(v0);
            _mesh.Vertices.Add(v1);
            _mesh.Vertices.Add(v2);
            _mesh.Vertices.Add(v3);

            _mesh.Indices.Add(baseIndex + 0);
            _mesh.Indices.Add(baseIndex + 1);
            _mesh.Indices.Add(baseIndex + 2);

            _mesh.Indices.Add(baseIndex + 0);
            _mesh.Indices.Add(baseIndex + 2);
            _mesh.Indices.Add(baseIndex + 3);
        }

    }
}
