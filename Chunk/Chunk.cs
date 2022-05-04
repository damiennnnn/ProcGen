using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using Blotch;
using System.Collections.Generic;
using penguin_new.Assets;

namespace penguin_new.ChunksBlocks
{
    public class Chunk
    {
        public static Vector3 ChunkSize = new Vector3(16, 256, 16);

        public Dictionary<Vector3, Block> Blocks = new Dictionary<Vector3, Block>(); // blocks within chunk
        public Vector3 Position = Vector3.Zero; // position of chunk

        BasicEffect _effect; // effect used for render
        public CubeMesh Mesh;
        /*				Matrix.CreateTranslation(1f,0,-1f),//Front
                        Matrix.CreateTranslation(-1f,0,-1f),//Back
                        Matrix.CreateTranslation(0,-1f,-1f),//Left
                        Matrix.CreateTranslation(0f,1f,-1f),//Right
                        Matrix.CreateTranslation(0f,0f,0f),//Top
                        Matrix.CreateTranslation(0f,0f,-2f)//Bottom*/

        public static Vector3[] GetSurroundingChunkCoords(Vector3 _chunkPos)
        {
            return new Vector3[]
            {
                    _chunkPos,
                    _chunkPos + new Vector3(1f, 0f, 0f),
                    _chunkPos + new Vector3(-1f, 0f, 0f),
                    _chunkPos + new Vector3(0f, -1f, 0f),
                    _chunkPos + new Vector3(0f, 1f, 0f),
                    _chunkPos + new Vector3(0f, 0f, 1f),
                    _chunkPos + new Vector3(0f, 0f, -1f)
            };
        }

        public bool CheckCollision(Entity _ent){
            bool _collide = false;

            Vector3 _relativeChunkPos = Vector.Extensions.Modulo(_ent.Position, ChunkSize);
            _ent._relativePos = _relativeChunkPos;
            BoundingBox _entBox = new BoundingBox(_relativeChunkPos, _relativeChunkPos + Vector3.One);
            Parallel.ForEach(Blocks, _keyValPair => {
                BoundingBox _box = new BoundingBox(_keyValPair.Key, _keyValPair.Key + Vector3.One);
                if (_box.Intersects(_entBox)){
                    _collide = true;
                }
            });
            return _collide;
        }

        public bool CheckCollision(Entity _ent, out BoundingBox[] _bounding)
        {
            /*_bounding = new BoundingBox();
            for (int i = 0; i < Mesh.Bounding.Length; i++)
            {
                if (Mesh.Bounding[i].Intersects(_ent.Bounding))
                {
                    _bounding = Mesh.Bounding[i];
                    return true;
                }
            }
            return false;*/
            bool _collide = false;

            Vector3 _relativeChunkPos = Vector.Extensions.Modulo(_ent.Position, ChunkSize);
            _ent._relativePos = _relativeChunkPos;
            BoundingBox _entBox = new BoundingBox(_relativeChunkPos, _relativeChunkPos + Vector3.One);
            List<BoundingBox> _result = new List<BoundingBox>();
            //CubeMesh.CubeBounding.Deconstruct(out Vector3 _min, out Vector3 _max);
            Vector3 _min = new Vector3(-1, -1, -2f);
            Vector3 _max = new Vector3(1, 1, 0f);
            Parallel.ForEach(Blocks, _keyValPair => {
                BoundingBox _box = new BoundingBox((_keyValPair.Key*2) + _min, (_keyValPair.Key*2) + _max);
                if (_box.Intersects(_entBox)){
                    _collide = true;
                    _result.Add(_box);
                }   
            });
            _bounding = _result.ToArray();;
            return _collide;
        }
        public void UpdateMesh(BlGraphicsDeviceManager _graphics, out int blockCount)
        {
            if (Mesh == null) Mesh = new CubeMesh();

            _effect = new BasicEffect(_graphics.GraphicsDevice);
            _effect.EnableDefaultLighting();
            _effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            _effect.SpecularColor = Color.Transparent.ToVector3();
            _effect.SpecularPower = 0.01f;
            _effect.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            _effect.LightingEnabled = true;


            ConcurrentBag<Block> blocksToRender = new ConcurrentBag<Block>();

            var keyValPairs = Blocks.AsEnumerable();

            Parallel.For(0, keyValPairs.Count(), x =>
             {
                 var block = keyValPairs.ElementAt(x);
                 var blockPos = block.Key;
                 bool[] facesToRender =
                 {
                        true, // forward
                        true, // back
                        true, // right
                        true, // left
                        true, // top
                        true // bottom
                     };
                 Vector3[] vecs =
                 {
                        blockPos + new Vector3(1f,0f,0f),
                        blockPos + new Vector3(-1f,0f,0f),
                        blockPos + new Vector3(0f,-1f,0f),
                        blockPos + new Vector3(0f,1f,0f),
                        blockPos + new Vector3(0f,0f,1f),
                        blockPos + new Vector3(0f,0f,-1f)
                     };
                 var chunkPos = (block.Value.GetWorldPosition() - blockPos) / ChunkSize;
                 chunkPos.Round();

                 var occluded = true;
                 for (int i = 0; i < 6; i++)
                 {
                     bool blockInPos = Blocks.TryGetValue(vecs[i], out var val); // is block in given position?

                     for (int z = 0; z < Main.MainWorld.ChunkArray.Length; z++)
                     {
                         Vector3 realWorld = vecs[i] + (chunkPos * ChunkSize); // check surrounding chunks, not just local chunk
                         realWorld /= (ChunkSize); // get block position in chunk
                         realWorld.Round();

                     }

                     if (!blockInPos) occluded = false; // if any side is visible, block cant be occluded
                     facesToRender[i] = !blockInPos; // render face if block not present, dont render face if block is
                 }
                 if (!occluded)
                 {
                     var blockReal = block.Value;
                     blockReal.SideVisible = facesToRender;
                     blocksToRender.Add(blockReal); // add block to render list only if it isnt occluded

                 }
             });

            blockCount = blocksToRender.Count;
            Mesh.MeshSprites = Mesh.CreateCubeMesh(blocksToRender.ToArray(), _graphics);
            ShouldUpdate = false;
        }

        
        private Object _lockObj = new object();
        private BoundingBox _box;
        private Vector3 _min = new Vector3(-1f,-1f,-1f);
        private Vector3 _max = new Vector3(1f,1f,1f);
        public void DrawMesh(ref BlGraphicsDeviceManager _graphics, out int draw, ref BasicEffect eff, ref BoundingFrustum _frustum, ref Player _player, int x = 0)
        {
            draw = 0;
            if (Mesh == null) return;
            if (_box == null) _box = new BoundingBox();
            for (int i = 0; i < Mesh.MeshSprites.Count(); i++)
            {
                Vector3 vecPos = Mesh.MeshSprites[i].Matrix.Translation;
                float _dist = Vector3.Distance(vecPos, _player.Position);
                if (_dist > 64f) continue; // only render block if its close enough
                
                Vector3.Add(ref _min, ref vecPos, out var _newMin);
                Vector3.Add(ref _max, ref vecPos, out var _newMax);
                _box.Min = _newMin;
                _box.Max = _newMax;

                _frustum.Intersects(ref _box, out bool _inView);
                if (!_inView) continue;

                draw += 1;
                eff.World = Mesh.MeshSprites[i].Matrix;
                eff.TextureEnabled = true;
                eff.Texture = (Texture2D)Mesh.MeshSprites[i].Mipmap;
                var buffer = CubeMesh.Buffers[Mesh.MeshSprites[i].Name];
                
                _graphics.GraphicsDevice.SetVertexBuffer(buffer);
                for (int z = 0; z < eff.CurrentTechnique.Passes.Count; z++)
                {
                    eff.CurrentTechnique.Passes[z].Apply();
                    _graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, buffer.VertexCount / 3);
                    draw++;
                }
            }
        }

        
        
        public Vector3 GetCol(int i)
        {
            switch (i % 4)
            {
                case 0: return Color.Red.ToVector3();
                case 1: return Color.Blue.ToVector3();
                case 2: return Color.Yellow.ToVector3();
                case 3: return Color.Purple.ToVector3();
                case 4: return Color.Green.ToVector3();
                default: return Color.Transparent.ToVector3();
            }
        }
        public void AddBlock(Block _block)
        {
            Blocks.Add(_block.Position, _block);
        }
        public bool ShouldUpdate { get => _hasUpdated; set => _hasUpdated = value; }
        private bool _hasUpdated = false;
    }
}
