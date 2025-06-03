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
    }

    public class Chunk
    {
        private static int chunkSize = 256;

        private Block[,,] blocks = new Block[chunkSize, chunkSize, chunkSize];
        private int _posVbo;
        private int _vbo;
        private int _ebo;
        private int _vao;
        private List<Vector3> blockPos = new List<Vector3>();
        List<Vertex> vertices = new List<Vertex>();
        List<int> indices = new List<int>();
        private int blockCount;

        private void greedyNX()
        {
            Vector3 dirU = new Vector3(0f, 0f, 1f);
            Vector3 dirV = new Vector3(0f, 1f, 0f);
            Vector3 norm = new Vector3(-1f, 0f, 0f);

            // we basically iterate through all of the faces. We then check if the face is occluded or was already included in another
            // quad. If not, we expand up, then we check if we can expand sideways. Then we just emit the quad    
            for (int x = 0; x < chunkSize; x++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (blocks[x, y, z].Active && (x == 0 || !blocks[x - 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }

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
                            if (y + vOffset < chunkSize && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z])
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
                                if (z + uOffset < chunkSize && y + i < chunkSize && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset])
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
                        emitQuad(quad);
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
            for (int x = 0; x < chunkSize; x++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (blocks[x, y, z].Active && (x == chunkSize - 1 || !blocks[x + 1, y, z].Active))
                        {
                            uncovered[y, z] = true;
                        }
                    }
                }

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, z] || !uncovered[y, z])
                        {
                            continue;
                        }

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
                            if (y + vOffset < chunkSize && uncovered[y + vOffset, z] && !inAQuad[y + vOffset, z])
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
                                if (z + uOffset < chunkSize && y + i < chunkSize && uncovered[y + i, z + uOffset] && !inAQuad[y + i, z + uOffset])
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
                        emitQuad(quad);
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
            for (int y = 0; y < chunkSize; y++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (y == chunkSize - 1 || !blocks[x, y + 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }

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
                            if (z + vOffset < chunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x])
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
                                if (x + uOffset < chunkSize && z + i < chunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset])
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
                        emitQuad(quad);
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
            for (int y = 0; y < chunkSize; y++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (y == 0 || !blocks[x, y - 1, z].Active))
                        {
                            uncovered[z, x] = true;
                        }
                    }
                }

                for (int z = 0; z < chunkSize; z++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[z, x] || !uncovered[z, x])
                        {
                            continue;
                        }

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
                            if (z + vOffset < chunkSize && uncovered[z + vOffset, x] && !inAQuad[z + vOffset, x])
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
                                if (x + uOffset < chunkSize && z + i < chunkSize && uncovered[z + i, x + uOffset] && !inAQuad[z + i, x + uOffset])
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
                        emitQuad(quad);
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
            for (int z = 0; z < chunkSize; z++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (z == 0 || !blocks[x, y, z - 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }

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
                            if (y + vOffset < chunkSize && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x])
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
                                if (x + uOffset < chunkSize && y + i < chunkSize && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset])
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
                        emitQuad(quad);
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
            for (int z = 0; z < chunkSize; z++)
            {
                bool[,] uncovered = new bool[chunkSize, chunkSize];
                bool[,] inAQuad = new bool[chunkSize, chunkSize];

                // check which faces are uncovered
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        if (blocks[x, y, z].Active && (z == chunkSize - 1 || !blocks[x, y, z + 1].Active))
                        {
                            uncovered[y, x] = true;
                        }
                    }
                }

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        // won't render a quad or expand if this face is covered or is already in another quad.
                        if (inAQuad[y, x] || !uncovered[y, x])
                        {
                            continue;
                        }

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
                            if (y + vOffset < chunkSize && uncovered[y + vOffset, x] && !inAQuad[y + vOffset, x])
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
                                if (x + uOffset < chunkSize && y + i < chunkSize && uncovered[y + i, x + uOffset] && !inAQuad[y + i, x + uOffset])
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
                        emitQuad(quad);
                    }
                }
            }
        }

        public void greedyMesh()
        {
            greedyNX();
            greedyPX();
            greedyPY();
            greedyNY();
            greedyNZ();
            greedyPZ();
        }

        private void emitQuad(Quad quad)
        {
            Vector3 pos0 = quad.Start;
            Vector3 pos1 = quad.Start + quad.DirU * quad.SizeU;
            Vector3 pos2 = quad.Start + quad.DirU * quad.SizeU + quad.DirV * quad.SizeV;
            Vector3 pos3 = quad.Start + quad.DirV * quad.SizeV;

            Vertex v0 = new Vertex() { pos = pos0, normal = quad.Normal, uv = new Vector2(0.0f, 0.0f) };
            Vertex v1 = new Vertex() { pos = pos1, normal = quad.Normal, uv = new Vector2(quad.SizeU, 0.0f) };
            Vertex v2 = new Vertex() { pos = pos2, normal = quad.Normal, uv = new Vector2(quad.SizeU, quad.SizeV) };
            Vertex v3 = new Vertex() { pos = pos3, normal = quad.Normal, uv = new Vector2(0.0f, quad.SizeV) };

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

        public Chunk() 
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {

                        blocks[x, y, z] = new Block();
                    }
                }
            }
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetSeed(1337);
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(0.0025f); 
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(5);       
            noise.SetFractalLacunarity(2.0f); 
            noise.SetFractalGain(0.5f);

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {

                    float value = noise.GetNoise(x, z) / 0.5f + 0.5f;

                    if(value > 1.0)
                    {
                        value = 1.0f;
                    }


                    for (int y = 0; y < 28; y++)
                    {
                        blocks[x, y, z].Active = true;
                        blocks[x, y, z].type = Block.Type.WATER;


                    }

                    for (int y = 0; y < chunkSize * value; y++)
                    {       
                        blocks[x, y, z].Active = true;
                        
                        if(y > 58)
                        {
                            blocks[x, y, z].type = Block.Type.SNOW;
                        }
                        
                    }
                }
            }

            greedyMesh();

            Console.WriteLine(vertices.Count);

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

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);
        }

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
