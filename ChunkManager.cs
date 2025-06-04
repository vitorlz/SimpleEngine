using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEngine.Voxels
{
    public class ChunkManager
    {
        private int _chunkSize = 64;

        private Vector2 _cameraPos;

        private FastNoiseLite _noise;

        // the amount of chunks we can see --> render chunks within a 16 * chunkSize distance from the camera.
        private int _renderDistance = 16;

        // maintain a list of chunks that should be rendered to the screen

        // this list is going to have size renderDistance * renderDistance. In the update function, the 

        private List<Chunk> _activeChunks = new List<Chunk>();

        // also maintain a map of chunks that we have already loaded so that we can recover their state when we have to render them again
        // chunks essentially live in a 2d grid (in the xz plane), so the index will just be x + z * (_renderDistance).

        private Dictionary<int, Chunk> _loadedChunks = new Dictionary<int, Chunk>();

        // populate _activeChunks with initial chunks based on the camera's initial position
        public ChunkManager(Vector3 cameraPos) 
        {

            FastNoiseLite _noise = new FastNoiseLite();
            var rand = new Random();
           _noise.SetSeed(rand.Next());
           _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
           _noise.SetFrequency(0.0025f);
           _noise.SetFractalType(FastNoiseLite.FractalType.FBm);
           _noise.SetFractalOctaves(5);
           _noise.SetFractalLacunarity(2.0f);
           _noise.SetFractalGain(0.5f);
            // figure out which chunk we are in now
            _cameraPos = new Vector2(cameraPos.X, cameraPos.Z);

            Vector2 currentChunkPos = (_cameraPos.Truncate() / _chunkSize).Truncate();

            for(int x = -(_renderDistance / 2); x <= _renderDistance / 2; x++)
            {
                for (int y = -(_renderDistance / 2); y <= _renderDistance / 2; y++)
                {
                    // send the chunk position to the chunk. In the chunk we offset everything by Pos.
                    Vector2 pos = new Vector2((currentChunkPos.X + x) * _chunkSize, (currentChunkPos.Y + y) * _chunkSize);
                    Chunk newChunk = new Chunk(pos, _chunkSize, _noise);
                    _activeChunks.Add(newChunk);
                    _loadedChunks[(int)pos.X + (int)pos.Y * 64] = newChunk;
                }
            }
        
        }

        public void Update(Vector3 cameraPos)
        {
            _cameraPos = new Vector2(cameraPos.X, cameraPos.Z);
            Vector2 currentChunkPos = (_cameraPos.Truncate() / _chunkSize).Truncate();

            for (int i = 0; i < _activeChunks.Count; i++)
            {
                if (Math.Abs(_activeChunks[i].Pos.X / _chunkSize - currentChunkPos.X)  > _renderDistance || Math.Abs(_activeChunks[i].Pos.Y / _chunkSize - currentChunkPos.Y) > _renderDistance)
                {
                    _activeChunks.RemoveAt(i);
                }

                
            }
        }

        //private bool isChunkVisible(Vector2 chunkPos)
        //{

        //    // Camera position in chunks: we can do this simple calculation because chunks are 1 by 1.
        //    Vector2 camPos = new Vector2(_cameraPos.X / _chunkSize, _cameraPos.Z / _chunkSize);

        //    return Math.Abs(chunkPos.X - camPos.X) <= _renderDistance && Math.Abs(chunkPos.Y - camPos.Y) <= _renderDistance;
        //}

        public void RenderActiveChunks()
        {
            foreach (var chunk in _activeChunks)
            {
                chunk.Render();
            }
        }

        private void UpdateActiveChunks()
        {
            
        }
    }
}
