using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using SimpleEngine.Cam;
using System.Collections.Concurrent;

namespace SimpleEngine.Voxels
{
    public class ChunkManager
    {
        private const int _chunkHeight = 100;
        private const int _chunkSize = 16;
        // the amount of chunks we can see --> render chunks within a _renderDistance * chunkSize distance from the camera.
        private const int _renderDistance = 32;
        private const int _chunksPerThread = 4;
        private const int _gpuUploadLimit = _renderDistance * 2;
        private const int _chunksUploadPerFrame = _renderDistance;

        private Camera _camera;
        private Vector2 _cameraPos;

        private Dictionary<string, FastNoiseLite> _noises = new Dictionary<string, FastNoiseLite>();
        // maintain a list of chunks that should be rendered to the screen

        // this list is going to have size renderDistance * renderDistance. In the update function, the 
        private List<Chunk> _activeChunks = new List<Chunk>();

        // also maintain a map of chunks that we have already loaded so that we can recover their state when we have to render them again
        // chunks essentially live in a 2d grid (in the xz plane), so the index will just be x + z * (_renderDistance).
        private Dictionary<int, Chunk> _loadedChunks = new Dictionary<int, Chunk>();

        // maintain a list of the positions of chunks to load. We don't want to render all newly visible chunks at once in a single frame,
        // so we will just maintain a list of them and load a number of them per frame. Will try to load each chunk in 
        // a different thread using a thread pool
        private ConcurrentQueue<Chunk> _chunksToLoad = new ConcurrentQueue<Chunk>();
        private ConcurrentQueue<Chunk> _readyToUpload = new ConcurrentQueue<Chunk>();

        private Vector2 _currentChunkPos = new Vector2();

        public ChunkManager(Camera camera) 
        {
            _noises["height"] = new FastNoiseLite();
            _noises["stone"] = new FastNoiseLite();
            _noises["tree"] = new FastNoiseLite();

            var rand = new Random();
            int seed = rand.Next();

            _noises["height"].SetSeed(seed);
            _noises["height"].SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            _noises["height"].SetFrequency(0.0015f);
            _noises["height"].SetFractalType(FastNoiseLite.FractalType.FBm);
            _noises["height"].SetFractalOctaves(8);
            _noises["height"].SetFractalLacunarity(2.0f);
            _noises["height"].SetFractalGain(0.5f);
            _noises["height"].SetFractalWeightedStrength(0.0f);

            _noises["stone"].SetSeed(seed);
            _noises["stone"].SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            _noises["stone"].SetFrequency(0.001f);
            
            _noises["tree"].SetSeed(seed);
            _noises["tree"].SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            _noises["tree"].SetFrequency(0.001f);
            // figure out which chunk we are in now
            _camera = camera;
            _cameraPos = new Vector2(_camera.Transform.position.X, _camera.Transform.position.Z);
        }

        public void EnqueueNewChunks()
        {
            for (int x = -_renderDistance; x <= _renderDistance; x++)
            {
                for (int y = -_renderDistance; y <= _renderDistance; y++)
                {
                    // send the chunk position to the chunk. In the chunk we offset everything by Pos.
                    Vector2 pos = new Vector2((_currentChunkPos.X + x) * _chunkSize, (_currentChunkPos.Y + y) * _chunkSize);
                    if (!_loadedChunks.ContainsKey((int)pos.X + (int)pos.Y * 10000))
                    {
                        Chunk newChunk = new Chunk(pos, _chunkSize, _chunkHeight, _noises, new GreedyMesher());
                        _chunksToLoad.Enqueue(newChunk);
                        _loadedChunks[(int)pos.X + (int)pos.Y * 10000] = newChunk;
                    }
                }
            }            
        }

        public void RemoveOutOfSightChunks()
        { 
            for (int i = 0; i < _activeChunks.Count; i++)
            {
                float distanceX = Math.Abs((int)(_activeChunks[i].Pos.X / _chunkSize) - _currentChunkPos.X);
                float distanceY = Math.Abs((int)(_activeChunks[i].Pos.Y / _chunkSize) - _currentChunkPos.Y);

                if (distanceX > _renderDistance || distanceY > _renderDistance)
                {
                    Chunk chunkToRemove = _activeChunks[i];
                    _loadedChunks.Remove((int)chunkToRemove.Pos.X + (int)chunkToRemove.Pos.Y * 10000);
                    _activeChunks.RemoveAt(i);
                }
            }
        }

        // We are going to load two chunks per frame, each in a different thread.
        // The worker threads are going to get chunks from the _chunksToLoad, call CreateChunk() and put the loaded
        // chunks in _readyToUpload.
        public void LoadNewChunks()
        {
            if(_readyToUpload.Count > _gpuUploadLimit)
            {
                return;
            }

            Task.Run(() =>
            {
                for(int i = 0; i < _chunksPerThread; i++)
                {
                    LoadChunk();
                }
            });
        }

        // create a chunk and put it in the ready to render queue. 
        // This is done by a worker thread.
        public void LoadChunk()
        {   
            Chunk chunkToLoad;
            if(_chunksToLoad.TryDequeue(out chunkToLoad))
            {
                chunkToLoad.CreateChunk();
                _readyToUpload.Enqueue(chunkToLoad);
            }
        }

        // upload new chunk to gpu. This has to be done by the main thread because it calls the opengl api.
        // put the now ready-to-render chunk in _activeChunks so that it can be rendered
        public void UploadNewChunk()
        {
            for (int i = 0; i < _chunksUploadPerFrame; i++)
            {
                Chunk chunkToUpload;
                if (_readyToUpload.TryDequeue(out chunkToUpload))
                {
                    chunkToUpload.UploadVerticesToGPU();
                    _activeChunks.Add(chunkToUpload);
                }
            }   
        }

        public void RenderActiveChunks()
        {
            for(int i = 0; i < _activeChunks.Count; i++) 
            {
                _activeChunks[i].Render();
            }
        }

        public void UpdateCurrentChunkPos()
        {
            _cameraPos.X = _camera.Transform.position.X;
            _cameraPos.Y = _camera.Transform.position.Z;
            _currentChunkPos = (_cameraPos / _chunkSize).Truncate();
        }

        public void Update()
        {
            UpdateCurrentChunkPos();
            RemoveOutOfSightChunks();
            EnqueueNewChunks();
            LoadNewChunks();
            UploadNewChunk();
        }
    }
}
