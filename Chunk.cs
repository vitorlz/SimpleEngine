using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;



namespace SimpleEngine.Voxels
{
    public class Chunk
    {
        private static int chunkSize = 64;

        private Block[,,] blocks = new Block[chunkSize, chunkSize, chunkSize];
        private int _posVbo;
        private int _vbo;
        private int _ebo;
        private int _vao;
        private List<Vector3> blockPos = new List<Vector3>();

        private int blockCount;
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
            noise.SetFrequency(0.01f); 
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

                        blockPos.Add(new Vector3(x, y, z));
                        blockCount++;

                    }


                    for (int y = 0; y < chunkSize * value; y++)
                    {       
                        blocks[x, y, z].Active = true;
                        
                        blockPos.Add(new Vector3(x, y, z));
                        blockCount++;
                        
                    }
                }
            }

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Cube.vertices.Length * sizeof(float), Cube.vertices, BufferUsageHint.StaticDraw);
           
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            _posVbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _posVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, blockCount * Vector3.SizeInBytes, blockPos.ToArray(), BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribDivisor(3, 1);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Cube.indices.Length * sizeof(float), Cube.indices, BufferUsageHint.StaticDraw);
        }

        public void Update()
        {

        }

        public void Render()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, Cube.indices.Length, DrawElementsType.UnsignedInt, 0, blockCount);
        }
    }
}
