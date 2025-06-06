using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;


namespace SimpleEngine.Voxels
{
    public struct Vertex
    {
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uv;
        public int type;
    }

    public class Chunk
    {
        private int _chunkSize;

        // The position of the chunk will be _chunkSize * Pos.x, _chunkSize * Pos.y
        public Vector2 Pos { get; set; }

        private Block[,,] blocks;
        private int _posVbo;
        private int _vbo;
        private int _ebo;
        private int _vao;
        private List<Vector3> blockPos = new List<Vector3>();
        List<Vertex> vertices = new List<Vertex>();
        List<int> indices = new List<int>();
        private FastNoiseLite _noise;
        private Vector3 _chunkOffset = new Vector3();

        private void greedyNX()
        {
            Vector3 dirU = new Vector3(0f, 0f, 1f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(-1f, 0f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int x = 0; x < _chunkSize; x++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        if (blocks[x, y, z].Active && (x == 0 || !blocks[x - 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }

                        // create a quad
                        Quad quad = new Quad(new Vector3(x, y, z), dirU, dirV, norm);
                        Block.Type neededType = blocks[x, y, z].type;
                        // width and height voxel offsets from starting position
                        int uOffset = 1;
                        int vOffset = 1;

                        bool expandV = true;
                        bool expandU = true;

                        // try to expand up. We see if the voxel face above is uncovered and active and not yet in a quad

                        while (expandV)
                        {
                            if (y + vOffset < _chunkSize && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z] && blocks[x, y + vOffset, z].type == neededType)
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
                                if (z + uOffset < _chunkSize && y + i < _chunkSize && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset] && blocks[x, y + i, z + uOffset].type == neededType)
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
                        
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }

        private void greedyPX()
        {
            Vector3 dirU = new Vector3(0f, 0f, 1f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(1f, 0f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int x = 0; x < _chunkSize; x++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        if (blocks[x, y, z].Active && (x == _chunkSize - 1 || !blocks[x + 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }
                        Block.Type neededType = blocks[x, y, z].type;
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
                            if (y + vOffset < _chunkSize && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z] && blocks[x, y + vOffset, z].type == neededType)
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
                                if (z + uOffset < _chunkSize && y + i < _chunkSize && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset] && blocks[x, y + i, z + uOffset].type == neededType)
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
                        
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }
        private void greedyPY()
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 0f, 1f);
            Vector3 norm = new Vector3(0f, 1f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int y = 0; y < _chunkSize; y++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int z = 0; z < _chunkSize; z++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (y == _chunkSize - 1 || !blocks[x, y + 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < _chunkSize; z++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }
                        Block.Type neededType = blocks[x, y, z].type;
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
                            if (z + vOffset < _chunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x] && blocks[x, y, z + vOffset].type == neededType)
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
                                if (x + uOffset < _chunkSize && z + i < _chunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset] && blocks[x + uOffset, y, z + i].type == neededType)
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
                                inAQuad[z + i,x + j] = true;
                            }
                        }

                        quad.SizeU = uOffset;
                        quad.SizeV = vOffset;
                       
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }

        private void greedyNY()
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 0f, 1f);
            Vector3 norm = new Vector3(0f, -1f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int y = 0; y < _chunkSize; y++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int z = 0; z < _chunkSize; z++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (y == 0 || !blocks[x, y - 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < _chunkSize; z++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }
                        Block.Type neededType = blocks[x, y, z].type;
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
                            if (z + vOffset < _chunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x] && blocks[x, y, z + vOffset].type == neededType)
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
                                if (x + uOffset < _chunkSize && z + i < _chunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset] && blocks[x + uOffset, y, z + i].type == neededType)
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
                        
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }

        private void greedyNZ()
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(0f, 0f, -1f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int z = 0; z < _chunkSize; z++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (z == 0 || !blocks[x, y, z - 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }
                        Block.Type neededType = blocks[x, y, z].type;
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
                            if (y + vOffset < _chunkSize && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x] && blocks[x, y + vOffset, z].type == neededType)
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
                                if (x + uOffset < _chunkSize && y + i < _chunkSize && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset] && blocks[x + uOffset, y + i, z].type == neededType)
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
                       
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }

        public void greedyPZ()
        {
            Vector3 dirU = new Vector3(1f, 0f, 0f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(0f, 0f, 1f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int z = 0; z < _chunkSize; z++)
            {
                bool[,] uncovered = new bool[_chunkSize, _chunkSize];
                bool[,] inAQuad = new bool[_chunkSize, _chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (z == _chunkSize - 1 || !blocks[x, y, z + 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int x = 0; x < _chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }
                        Block.Type neededType = blocks[x, y, z].type;
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
                            if (y + vOffset < _chunkSize && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x] && blocks[x, y + vOffset, z].type == neededType)
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
                                if (x + uOffset < _chunkSize && y + i < _chunkSize && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset] && blocks[x + uOffset, y + i, z].type == neededType)
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
                        
                        emitQuad(quad, (int)blocks[x, y, z].type);
                    }
                }
            }
        }

        public void GreedyMesh()
        {
            greedyNX();
            greedyPX();
            greedyPY();
            greedyNY();
            greedyNZ();
            greedyPZ();
        }

        private void emitQuad(Quad quad, int type)
        {
            Vector3 offset = new Vector3(Pos.X, 0, Pos.Y); 

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

            int baseIndex = vertices.Count;
            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            indices.Add(baseIndex + 0);
            indices.Add(baseIndex + 1);
            indices.Add(baseIndex + 2);

            indices.Add(baseIndex + 0);
            indices.Add(baseIndex + 2);
            indices.Add(baseIndex + 3);
        }

        public Chunk(Vector2 pos, int chunkSize, FastNoiseLite noise)
        {
            Pos = pos;
            _chunkSize = chunkSize;
            _noise = noise;
            blocks = new Block[_chunkSize, _chunkSize, _chunkSize];
            _chunkOffset = new Vector3(Pos.X, 0, Pos.Y);

        }

        public void CreateChunk()
        {
            for (int x = 0; x < _chunkSize; x++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {

                        blocks[x, y, z] = new Block();
                    }
                }
            }

            for (int x = 0; x < _chunkSize; x++)
            {
                for (int z = 0; z < _chunkSize; z++)
                {
                    float value = _noise.GetNoise(x + _chunkOffset.X, z + _chunkOffset.Z) / 0.5f + 0.5f;

                    if (value > 1.0)
                    {
                        value = 1.0f;
                    }

                    for (int y = 0; y <= 10; y++)
                    {
                        blocks[x, y, z].Active = true;
                        blocks[x, y, z].type = Block.Type.WATER;
                    }

                    for (int y = 0; y < _chunkSize * value; y++)
                    {
                        blocks[x, y, z].Active = true;

                        if (y > 28)
                        {
                            blocks[x, y, z].type = Block.Type.SNOW;
                        }
                        else if (y > 10)
                        {
                            blocks[x, y, z].type = Block.Type.GRASS;
                        }
                        else
                        {
                            blocks[x, y, z].type = Block.Type.WATER;
                        }
                    }
                }
            }

            GreedyMesh();
        }

        public void UploadVerticesToGPU()
        {
            int vertexSize = Marshal.SizeOf(typeof(Vertex));
            int totalSize = vertices.Count * vertexSize;

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, totalSize, vertices.ToArray(), BufferUsageHint.StaticDraw);

            // Set up vertex attributes (layout must match your Vertex struct)
            int stride = Marshal.SizeOf(typeof(Vertex));
            int offset = 0;

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, offset);
            offset += Vector3.SizeInBytes;

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, offset);
            offset += Vector3.SizeInBytes;

            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, offset);
            offset += Vector2.SizeInBytes;

            GL.EnableVertexAttribArray(3);
            GL.VertexAttribIPointer(3, 1, VertexAttribIntegerType.Int, stride, offset);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);
        }

        // re-mesh and reupload vertices to vbo when the chunk changes.
        public void Update()
        {
           
        }

        public void Render()
        {  
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
}
