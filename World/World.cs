using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using penguin_new.ChunksBlocks;
using System.Diagnostics;

namespace penguin_new
{
    public class World
    {


        private int _WorldSeed = 10000000;

        public string Seed { get => _WorldSeed.ToString(); }
        public string WorldName { get; set; }
        public ConcurrentDictionary<Vector3, Chunk> Chunks;

        public Chunk[] ChunkArray;

        public Entity[] Entities = new Entity[0]; // arrays give a slight performance advantage that adds up in real time calculations

        /// <summary>
        /// Player entity is stored with key "player", other entities are stored with unique identifiers.
        /// </summary>
        public Dictionary<string, Entity> EntityList = new Dictionary<string, Entity>();
        /// <summary>
        /// Has our world been set up?
        /// </summary>
        public bool WorldInitialised = false;
        public bool InitialiseMesh = false;
        private int _genProgress = 0;
        private int _maxGenProgress = 0;

        public int _meshProgress = 0;
        private int _maxMeshProgress = 0;

        public int BlockCount { get; set; }

        private SaveLoad.SaveManager _saveMan;
        private enum GenStatus
        {
            None,
            ChunkGen,
            MeshGen,
            WorldLoad
        }

        public int Status { get => (int)_currentStatus; }
        public string GetStatusMessage()
        {
            string _status = "";
            switch (_currentStatus)
            {
                case GenStatus.None: break;
                case GenStatus.ChunkGen: _status = string.Format("Generating chunks... ({0}/{1})", _genProgress, _maxGenProgress); break;
                case GenStatus.MeshGen: _status = string.Format("Generating meshes... ({0}/{1})", _meshProgress, _maxMeshProgress); break;
                case GenStatus.WorldLoad: _status = string.Format("Loading {0}...", WorldName); break;
            }
            return _status;
        }

        private GenStatus _currentStatus = GenStatus.None;

        public int GetMeshProgress()
        {
            if (_maxMeshProgress == 0) return 0;
            float _prog = ((float)_meshProgress / (float)_maxMeshProgress) * 100;
            return (int)_prog;
        }
        public int GetGenerationProgress()
        {
            if (_maxGenProgress == 0) return 0;
            float _prog = ((float)_genProgress / (float)_maxGenProgress) * 100;
            return (int)_prog;
        }

        public Player GetLocalPlayer() // gets the entity representing the current user's player ent
        {
            return (EntityList["player"] as Player);
        }
        public void AddEntity(Entity _ent, string _uid)
        {
            EntityList.Add(_uid, _ent);
        }
        public void AddEntity(Entity _ent)
        {
            var _prevArray = Entities;
            int length = (Entities.Length + 1);
            Entities = new Entity[length];

            for (int i = 0; i < (length - 2); i++)
                Entities[i] = _prevArray[i];

            Entities[length - 1] = _ent;
        }

        public World()
        {
            Random RNG = new Random();
            _WorldSeed = RNG.Next(10000000, 99999999);
        }

        public void AddChunkDictionary(Dictionary<Vector3, Chunk> _dic){
            foreach (var keyVal in _dic){
                if (Chunks.TryAdd(keyVal.Key, keyVal.Value))
                    Chunks[keyVal.Key].UpdateMesh(Main.GlobalGraphics, out int i );
            }

            ChunkArray = Chunks.Values.ToArray();
        }
        public bool LoadChunk(Vector3 _chunkPos){
            if (SaveLoad.ChunkGrabber.GetChunk(_chunkPos, _saveMan, out var _chunk)){
                _chunk.UpdateMesh(Main.GlobalGraphics, out int i);
                Chunks.TryAdd(_chunkPos, _chunk);
                ChunkArray = Chunks.Values.ToArray();
            }
            return false;
        }
        public bool Load(string _path)
        {
            Chunks = new ConcurrentDictionary<Vector3, Chunk>();

try{
            _saveMan = new SaveLoad.SaveManager(_path);
            string _worldInfo = File.ReadAllText(Path.Combine(_path, "world.json"));
            SaveLoad.SaveManager.WorldData _worldData = JsonConvert.DeserializeObject<SaveLoad.SaveManager.WorldData>(_worldInfo);
            WorldName = _worldData.Name;
            _WorldSeed = int.Parse(_worldData.Seed);

}
catch (Exception e){
    return false;
}

            int maxX = 2;
            int maxZ = 2;

            _maxGenProgress = (maxX*maxZ);
            _genProgress = 0;
            _currentStatus = GenStatus.WorldLoad;
            for (int x = -(maxX/2); x < (maxX/2); x++) // loads surrounding chunks in a 4x4 area from spawn
            {
                for (int z = -(maxZ/2); z < (maxZ/2); z++)
                {
                    _genProgress++;
                    Vector3 _pos = new Vector3(x, 0, z);
                    if (!SaveLoad.ChunkGrabber.GetChunk(_pos, _saveMan, out var chunk)) continue; // skip if the chunk doesnt exist
                    Chunks.TryAdd(_pos, chunk);
                    ChunkArray = Chunks.Values.ToArray();
                }
            }
            

            _currentStatus = GenStatus.MeshGen;
            _maxMeshProgress = ChunkArray.Length;
            for (int i = 0; i < ChunkArray.Length; i++)
            {
                ChunkArray[i].UpdateMesh(Main.GlobalGraphics, out int blockCount); // update each mesh
                BlockCount += blockCount;
                _meshProgress++;
            }

            Player player = new Player(Main.GlobalGraphics);
            AddEntity(player, "player");
            AddEntity(new Entity(Assets.ModelID.Penguin, new Vector3(-5,0,0), "bit", Main.GlobalGraphics), "bitch");

            WorldInitialised = true;
            InitialiseMesh = true;
            return true;
        }
        public Chunk CheckChunkCollisions(Entity _ent, out BoundingBox[] _bounding){
            _bounding = new BoundingBox[0];
            Vector3 _chunkPos = (_ent.Position / Chunk.ChunkSize);
            _chunkPos.Floor();

            Vector3[] _chunkVecs = Chunk.GetSurroundingChunkCoords(_chunkPos);

            for (int i =0; i < _chunkVecs.Length; i++){
                if (Chunks.TryGetValue(_chunkVecs[i], out Chunk _chnk)){
                    if (_chnk.CheckCollision(_ent, out _bounding)){
                        return _chnk; // returns chhunk if one is found with a block that collides with entity 
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Returns the 9 chunks surrounding an entity in a specified position.
        /// </summary>
        /// <param name="_pos">Position of entity in world</param>
        /// <returns></returns>
        public Dictionary<Vector3,Chunk> GetSurroundingChunks(Vector3 _pos)
        {
            Dictionary<Vector3,Chunk> _chunks = new Dictionary<Vector3,Chunk>();

            Vector3 _chunkPos = (_pos / Chunk.ChunkSize); _chunkPos.Round();

            Vector3[] _chunkVecs = Chunk.GetSurroundingChunkCoords(_chunkPos);

            for (int i = 0; i < _chunkVecs.Count(); i++)
            {
                if (Chunks.TryGetValue(_chunkVecs[i], out Chunk _chnk))
                {
                    _chunks.TryAdd(_chunkVecs[i], _chnk);
                }
            }

            return _chunks;
        }
        /// <summary>
        /// Saves the chunk in specified position
        /// </summary>
        /// <param name="_pos">Position of chunk to save</param>
        public void SaveChunk(Vector3 _pos)
        {
            foreach (var chnk in Chunks)
            {
                SaveLoad.ChunkGrabber.SaveChunk(chnk.Value, ref _saveMan);
            }

        }
        public void OpenWorldDirectory(){
            Process.Start("explorer.exe", _saveMan.SaveDir);
        }

        public void CreateChunk(Vector3 _chunkPos){
                Chunk cnk = new Chunk();
                cnk.Position = _chunkPos;
                PerlinChunkGeneration(ref cnk); // use our perlin noise algorithm to generate a chunk 
                cnk.UpdateMesh(Main.GlobalGraphics, out int i );

                Chunks.TryAdd(_chunkPos, cnk);
        }

        public void GenerateNew(int maxX = 4, int maxZ = 4, string _seed = "", string _name = " ")
        {
            if (maxX < 2) maxX = 2;
            if (maxZ < 2) maxZ = 2;
            if (!string.IsNullOrEmpty(_seed))
            {
                if (int.TryParse(_seed, out int newSeed))
                    _WorldSeed = newSeed;
                else
                    _WorldSeed = _seed.GetHashCode();
            }
            if (string.IsNullOrEmpty(_name))
                _name = "Default";
            WorldName = _name;
            var _Chunks = new ConcurrentDictionary<Vector3, Chunk>();

            _maxGenProgress = (maxX * maxZ * (int)Chunk.ChunkSize.X * (int)Chunk.ChunkSize.Z * 32);

            _currentStatus = GenStatus.ChunkGen;


            Parallel.For(-(maxX/2), (maxX/2), x =>
            {
                for (int z = -(maxX/2); z < (maxZ/2); z++)
                {
                    Chunk cnk = new Chunk();
                    Vector3 _pos = new Vector3(x, 0f, z);
                    cnk.Position = _pos;
                    PerlinChunkGeneration(ref cnk); // use our perlin noise algorithm to generate a chunk 
                    _Chunks.TryAdd(_pos, cnk);
                }
            });

            ChunkArray = _Chunks.Values.ToArray();
            Chunks = _Chunks;
            _currentStatus = GenStatus.MeshGen;
            _maxMeshProgress = ChunkArray.Length;

            for (int i = 0; i < _maxMeshProgress; i++)
            {
                ChunkArray[i].UpdateMesh(Main.GlobalGraphics, out int blockCount); // create a mesh for optimised rendering
                BlockCount += blockCount;
                _meshProgress++;
            }
            WorldInitialised = true;
            InitialiseMesh = true;

            _saveMan = new SaveLoad.SaveManager(this, _name);

        }


        private Block CreateBlock(Vector3 _pos, Vector3 _chunkPos, Assets.TextureID _id) // create a new block with our specific parameters
        {
            _pos.Round();
            Block _block = new Block();
            _block.SetTexture(_id);
            _block.Position = _pos;
            _block.ChunkPosition = _chunkPos;

            return _block;
        }

        float min = 0;
        float max = 1f;
        private void PerlinCreateBlock(float _threshold, Vector3 _pos, ref Chunk _chunk, Assets.TextureID _id, int _octaves = 20) // avoids reusing code
        {
            float _noise = Perlin.NoiseMaker.Noise((_chunk.Position + _pos) / 10, _octaves, ref min, ref max);
            _genProgress += 1;
            if (_noise < _threshold) return; // dont create the block if the noise is too small
            _chunk.AddBlock(CreateBlock(_pos, _chunk.Position, _id));
        }
        private void PerlinChunkGeneration(ref Chunk _chunk)
        {
            int chunkSeed = (_WorldSeed);
            Perlin.NoiseMaker.Reseed(chunkSeed); // use our world seed for the perlin noise algorithm

            for (int x = 0; x < Chunk.ChunkSize.X; x++)
            {
                for (int z = 0; z < Chunk.ChunkSize.Z; z++)
                {
                    for (int y = -32; y < 0; y++)
                    {
                        Vector3 _pos = new Vector3(x, y, z); // monogame vectors use the Z-axis as their up axis

                        switch (y)
                        {
                            //case <= 0: PerlinCreateBlock(0.01f, _pos, ref _chunk, Assets.TextureID.Samren); break;
                            case < -16 and > -30: PerlinCreateBlock(0.005f, _pos, ref _chunk, Assets.TextureID.Stone); break;
                            case >= -16 and <= -8: PerlinCreateBlock(0.005f, _pos, ref _chunk, Assets.TextureID.Dirt); break;
                            case >= -8 and <= 0: PerlinCreateBlock(0.0075f, _pos, ref _chunk, Assets.TextureID.Dirt); break;
                            case < -30: PerlinCreateBlock(0.005f, _pos, ref _chunk, Assets.TextureID.Stone); break;
                            default: _chunk.AddBlock(CreateBlock(_pos, _chunk.Position, Assets.TextureID.Stone)); break;
                            // take Y level and generate different blocks with different thresholds based on ylevel
                        }
                    }
                }
            
        }
    }
}}
