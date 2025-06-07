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
using System.Runtime.ExceptionServices;


namespace SimpleEngine.Voxels
{
    public class Chunk
    {
        private int _chunkSize;
        public int ChunkSize { get { return _chunkSize; } }
        private readonly IChunkMesher _chunkMesher;

        // The position of the chunk will be _chunkSize * Pos.x, _chunkSize * Pos.y
        public Vector2 Pos { get; set; }
        private Mesh _mesh = new Mesh();
        private Block[,,] _blocks;
        public Block[,,] Blocks { get { return _blocks; } }

        private int _vbo;
        private int _ebo;
        private int _vao;

        private Dictionary<string, FastNoiseLite> _noises;        
        private Vector3 _chunkOffset = new Vector3();

        public Chunk(Vector2 pos, int chunkSize, Dictionary<string, FastNoiseLite> noises, IChunkMesher chunkMesher)
        {
            Pos = pos;
            _chunkSize = chunkSize;
            _noises = noises;
            _blocks = new Block[_chunkSize, _chunkSize, _chunkSize];
            _chunkOffset = new Vector3(Pos.X, 0, Pos.Y);
            _chunkMesher = chunkMesher;
        }

        public void CreateChunk()
        {
            InitializeBlocks();
            CreateLandscape();
            _mesh = _chunkMesher.GenerateMesh(this); 
        }

        public void InitializeBlocks()
        {
            for (int x = 0; x < _chunkSize; x++)
            {
                for (int y = 0; y < _chunkSize; y++)
                {
                    for (int z = 0; z < _chunkSize; z++)
                    {
                        _blocks[x, y, z] = new Block();
                    }
                }
            }
        }

        public void CreateLandscape()
        {
            Random random = new Random();

            for (int x = 0; x < _chunkSize; x++)
            {
                for (int z = 0; z < _chunkSize; z++)
                {
                    Vector2 chunkPos = new Vector2(x + _chunkOffset.X, z + _chunkOffset.Z);

                    float heightNoise = Math.Clamp(_noises["height"].GetNoise(chunkPos.X, chunkPos.Y) / 0.5f + 0.5f, 0.0f, 1.0f);
                    float stoneNoise = Math.Clamp(_noises["stone"].GetNoise(chunkPos.X, chunkPos.Y) / 0.5f + 0.5f, 0.0f, 1.0f);
                    float treeNoise = Math.Clamp(_noises["tree"].GetNoise(chunkPos.X, chunkPos.Y) / 0.5f + 0.5f, 0.0f, 1.0f);

                    for (int y = 0; y <= _chunkSize * 0.1; y++)
                    {
                        CreateBlock(Block.Type.WATER, x, y, z);
                    }

                    for (int y = 0; y < _chunkSize * heightNoise; y++)
                    {
                        if (y > _chunkSize * 0.9)
                        {
                            CreateBlock(Block.Type.SNOW, x, y, z);
                        }
                        else if (y > _chunkSize * 0.1)
                        {
                            if (stoneNoise > 0.7 && y > _chunkSize * 0.7)
                            {
                                CreateBlock(Block.Type.STONE, x, y, z);
                            }
                            else
                            {
                                CreateBlock(Block.Type.GRASS, x, y, z);
                                if (y == (int)(_chunkSize * heightNoise))
                                {
                                    if (treeNoise > 0.3)
                                    {
                                        if (random.Next(100) == 1)
                                        {
                                            CreateTree(x, y, z);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateTree(int x , int y, int z)
        {
            int treeHeight = 3;
            y++;

            if (!(x > 0 && x < _chunkSize - 1 && z > 0 && z < _chunkSize - 1 && y + treeHeight + 1 < _chunkSize - 1))
            {
                return;
            }

            // trunk
            for (int i = 0; i < treeHeight; i++)
            {
                CreateBlock(Block.Type.WOOD, x, y + i, z);
            }

            // first leaves layer
            CreateBlock(Block.Type.LEAVES, x + 1, y + treeHeight, z);
            CreateBlock(Block.Type.LEAVES, x - 1, y + treeHeight, z);
            CreateBlock(Block.Type.LEAVES, x, y + treeHeight, z + 1);
            CreateBlock(Block.Type.LEAVES, x, y + treeHeight, z - 1);

            // top leaves layer
            CreateBlock(Block.Type.LEAVES, x, y + treeHeight + 1, z);
        }

        public void CreateBlock(Block.Type type, int x, int y, int z)
        {
            if(x < 0 || x > _chunkSize - 1 || z < 0 || z > _chunkSize - 1 || y < 0 || y > _chunkSize - 1)
            {
                return;
            }

            ref Block block = ref _blocks[x, y, z];
            block.type = type;
            block.Active = true;
        }

        public void UploadVerticesToGPU()
        {
            int vertexSize = Marshal.SizeOf(typeof(Vertex));
            int totalSize = _mesh.Vertices.Count * vertexSize;

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, totalSize, _mesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);

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
            GL.BufferData(BufferTarget.ElementArrayBuffer, _mesh.Indices.Count * sizeof(int), _mesh.Indices.ToArray(), BufferUsageHint.StaticDraw);
        }

        // re-mesh and reupload vertices to vbo when the chunk changes.
        public void Update()
        {
           
        }

        public void Render()
        {  
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _mesh.Indices.Count, DrawElementsType.UnsignedInt, 0);
        }
    }
}
